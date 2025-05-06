using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using OpenAI.Chat;
using Polly;
using Polly.Retry;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using AINovelWriter.Shared.Models.Enums;
using Microsoft.SemanticKernel.Connectors.Amazon;
using OpenAI.Audio;
using BinaryContent = System.ClientModel.BinaryContent;


namespace AINovelWriter.Shared.Services;

public class NovelWriterService : INovelWriter
{
	public event Action<string>? SendOutline;
    public event EventHandler<ChapterEventArgs>? SendChapterText;
    public event EventHandler<string>? TextToImageUrl;
    public event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
    public event EventHandler<AudioState>? SendAudioStateUpdate;
    public static ILoggerFactory _loggerFactory;
    private AppState appState;
    private IConfiguration configuration;
    public NovelWriterService(IConfiguration configuration, AppState appState, ILoggerFactory loggerFactory)
    {
        this.appState = appState;
        _loggerFactory = loggerFactory;
        this.appState = appState;
        this.configuration = configuration;
    }
    public async Task<NovelInfo> ReverseEngineerNovel(string epubFileData, string title)
    {
        var chapters = ReverseWriterService.ParseEpubChapters(epubFileData);
        var kernel = CreateKernel();
        var chapterOuts = new List<ChapterOutline>();
        var settings = new OpenAIPromptExecutionSettings() { MaxTokens = 2048 };
        Console.WriteLine($"ReverseEngineerNovel Chapters: {chapters.Count}");
        foreach (var chapter in chapters)
        {
            Console.WriteLine($"Chapter Tokens: {StringHelpers.GetTokens200K(chapter.Text)}");
            var args = new KernelArguments(settings) { ["novel_chapter"] = chapter.Text };
            var chapterOutline = await kernel.InvokePromptAsync<string>(Prompts.OutlineReversePrompt, args);
            chapter.Outline = chapterOutline;
            chapterOuts.Add(new ChapterOutline($"Chapter {chapter.Chapter}: {chapter.Title}", chapter.Outline, chapters.IndexOf(chapter)+1) { FullText = chapter.Text });
        }
        var novelInfo = new NovelInfo
        {
            Outline = string.Join("\n", chapters.Select(x => x.Outline)),
            Text = string.Join("\n", chapters.Select(x => x.Text)),
            ChapterOutlines = chapterOuts,
            Title = title
        };
        const string prompt = """
                              You are a novel Summarizer. Given a novel outline, provide a summary using the following json output format:
                              ## Output
                              Your output must be in json using the following format:
                              ```json
                              {
                              	"Theme": "Theme of the Novel described in 1-3 sentances",
                                "Characters": "paragraph describing 3 - 5 main Characters",
                                "PlotEvents": "paragraph describing 3 - 5 primary Plot Events"
                              }
                              ```
                              
                              ## Novel Outline
                              
                              {{ $novel }}
                              """;

        var novelArgs = new KernelArguments(settings) { ["novel"] = novelInfo.Outline };
        var json = await kernel.InvokePromptAsync<string>(prompt, novelArgs);
        //var concepts = JsonSerializer.Deserialize<NovelConcepts>(json.Replace("```json", "").Replace("```", "").Trim('\n'));
        //concepts.Title = title;
        novelInfo.ConceptDescription = json;
        //var args = new KernelArguments() { ["novel_chapter"] = JsonSerializer.Serialize(chapters) };
        //var novelInfo = await reverseEngineerFunc.InvokeAsync<NovelInfo>(kernel, args);
        return novelInfo;
    }
    public async Task<NovelConcepts> GenerateNovelIdea(GenreCategory genre, List<Genre> subgenres, NovelLength length,
        NovelTone tone, NovelAudience audience)
    {
        var random = new Random();
        var roll = random.Next(1, 9);
        var aIModel = roll switch
        {
            1 => AIModel.Gpt41Mini,
            2 => AIModel.Gpt4OMini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt41,
            //5 => AIModel.OpenMistralNemo,
            5 => AIModel.Gemini15,
            6 => AIModel.Gpt4OCurrent,
            7 => AIModel.Gpt41Nano,
            8 => AIModel.Grok3,
            _ => AIModel.Gpt4OMini
        };
       
        var kernel = CreateKernel(aIModel);
        var rng = new Random();
        var personality = Enum.GetValues<Personality>()[rng.Next(Enum.GetValues<Personality>().Length)];
        
        var personality2 = Enum.GetValues<Personality>()[rng.Next(Enum.GetValues<Personality>().Length)];
        var personalityVar =
            $"**{personality.ToString()}** as in _{personality.GetDescription()}_ or **{personality2.ToString()}** as in _{personality2.GetDescription()}_ .";
        var lengthDescription = length.GetDescription();

        var settings = GetPromptExecutionSettingsFromModel(aIModel, 0.9);
        var subgenreString = string.Join("\n", subgenres.Select(x => x.ToString()));
        Console.WriteLine($"Subgenres: \n---------------------\n{subgenreString}\n------------------------------------\n");
        var args = new KernelArguments(settings)
        {
            ["genre"] = $"{genre.GetDisplayName()}\n{genre.GetDescription()}", ["subgenre"] = subgenreString, ["personalities"] = personalityVar, ["lengthDescription"] = lengthDescription, ["tone"] = $"{tone} - {tone.GetDescription()}", ["audience"] = $"{audience} - {audience.GetDescription()}"
        };
        var json = await kernel.InvokePromptAsync<string>(Prompts.IdeaGeneratePrompt, args);
        Console.WriteLine($"Novel Idea:\n------------------------------------\n {json}\n------------------------------------\n");
        NovelConcepts? concepts;
        var rawJson = json.Replace("```json", "").Replace("```", "").Trim('\n');
        try
        {
            concepts = JsonSerializer.Deserialize<NovelConcepts>(rawJson);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Json Exception: {ex.Message}");
            var repairKernel = CreateKernel();
            var repairSettings = GetPromptExecutionSettingsFromModel(AIModel.Gpt4OMini, 0.9);
            var repairArgs = new KernelArguments(repairSettings) { ["concepts"] = rawJson };
            var repairJson = await repairKernel.InvokePromptAsync<string>(Prompts.IdeaRepairPrompt, repairArgs);
            concepts = JsonSerializer.Deserialize<NovelConcepts>(repairJson!);
        }

        concepts.Genre = genre;
        concepts.SubGenres = subgenres;
        return concepts;

    }

    public async Task<string> GenerateNovelTitle(NovelConcepts concepts)
    {
        var random = new Random();
        var roll = random.Next(1, 9);
        var aIModel = roll switch
        {
            1 => AIModel.Gpt41Mini,
            2 => AIModel.Gpt4OMini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt41,
            //5 => AIModel.OpenMistralNemo,
            5 => AIModel.Gemini15,
            6 => AIModel.Gpt4OCurrent,
            7 => AIModel.Gpt41Nano,
            8 => AIModel.Grok3,
            _ => AIModel.Gpt4OMini
        };

        var kernel = CreateKernel(aIModel);
        concepts.Title = "";
        var args = new KernelArguments() { ["availableInformation"] = concepts.AvailableInformation() };
        var title = await kernel.InvokePromptAsync<string>(Prompts.TitleGeneratePrompt, args);
        Console.WriteLine($"Novel Title:\n------------------------------------\n {title}\n------------------------------------\n");
        return title ?? "Nope! No title for you!";
    }

    public async Task<string> GenerateNovelDescription(NovelConcepts concepts)
    {
        var random = new Random();
        var roll = random.Next(1, 9);
        var aIModel = roll switch
        {
            1 => AIModel.Gpt41Mini,
            2 => AIModel.Gpt4OMini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt41,
            5 => AIModel.Gemini15,
            6 => AIModel.Gpt4OCurrent,
            7 => AIModel.Gpt41Nano,
            8 => AIModel.Grok3,
            _ => AIModel.Gpt4OMini
        };

        var kernel = CreateKernel(aIModel);
        concepts.Theme = "";
        var args = new KernelArguments() { ["availableInformation"] = concepts.AvailableInformation() };
        var description = await kernel.InvokePromptAsync<string>(Prompts.ThemeOrDescriptionGenPrompt, args);
        return description ?? "Nope! No description for you!";
    }

    public async Task<string> GenerateNovelCharacters(NovelConcepts concepts)
    {
        var random = new Random();
        var roll = random.Next(1, 9);
        var aIModel = roll switch
        {
            1 => AIModel.Gpt41Mini,
            2 => AIModel.Gpt4OMini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt41,
            5 => AIModel.Gemini15,
            6 => AIModel.Gpt4OCurrent,
            7 => AIModel.Gpt41Nano,
            8 => AIModel.Grok3,
            _ => AIModel.Gpt4OMini
        };

        var kernel = CreateKernel(aIModel);
        concepts.Characters = "";
        var args = new KernelArguments() { ["availableInformation"] = concepts.AvailableInformation() };
        var characterDetails = await kernel.InvokePromptAsync<string>(Prompts.CharacterGenPrompt, args);
        return characterDetails ?? "Nope! No description for you!";
    }

    public async Task<string> GenerateNovelPlotEvents(NovelConcepts concepts)
    {
        var random = new Random();
        var roll = random.Next(1, 9);
        var aIModel = roll switch
        {
            1 => AIModel.Gpt41Mini,
            2 => AIModel.Gpt4OMini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt41,
            5 => AIModel.Gemini15,
            6 => AIModel.Gpt4OCurrent,
            7 => AIModel.Gpt41Nano,
            8 => AIModel.Grok3,
            _ => AIModel.Gpt4OMini
        };

        var kernel = CreateKernel(aIModel);
        concepts.PlotEvents = "";
        var args = new KernelArguments() { ["availableInformation"] = concepts.AvailableInformation() };
        var plotEvents = await kernel.InvokePromptAsync<string>(Prompts.PlotEventGenPrompt, args);
        return plotEvents ?? "Nope! No description for you!";
    }

    public async Task<string> CreateNovelOutline(NovelConcepts concepts)
    {
        var aIModel = concepts.OutlineAIModel;
        var novelTitle = concepts.Title;
        var theme = concepts.Description;
        var characterDetails = concepts.Characters;
        var plotEvents = concepts.PlotEvents;
        var chapters = concepts.ChapterCount;
        var additionalInstructions = concepts.AdditionalInstructions;
        var kernel = CreateKernel(aIModel);
        var novelWriter = new NovelWriterPlugin(aIModel);
        var plugin = kernel.ImportPluginFromObject(novelWriter);
        var createOutlineFunc = plugin["CreateNovelOutline"];
        var settings = GetOutlinePromptExecutionSettingsFromModel(aIModel);
        var args = new KernelArguments(settings)
        {
            ["theme"] = theme,
            ["characterDetails"] = characterDetails,
            ["plotEvents"] = plotEvents,
            ["novelTitle"] = novelTitle,
            ["chapters"] = chapters,
            ["audience"] = concepts.Audience,
            ["tone"] = concepts.Tone,
        };
        var instructions = string.IsNullOrEmpty(additionalInstructions) ? "" : $"## Additional User Instructions\n\n{additionalInstructions}\n";
        args["additionalInstructions"] = instructions;
       
        var outline = await createOutlineFunc.InvokeAsync<string>(kernel, args);
        _description = concepts.ToString();
        
        return outline;
    }

    private string _description = "";

    public async IAsyncEnumerable<string> WriteFullNovel(string outline, AIModel aiModel = AIModel.Gpt41,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        //var chapterOutline = JsonSerializer.Deserialize<ChapterOutline>(outline.Replace("```json","").Replace("```","").Trim('\n'));
        var chapters = SplitMarkdownByHeaders(outline);
        var kernel = CreateKernel(aiModel);
        var summerizeKernel = CreateKernel(AIModel.Gpt41Mini);
        var novelPlugin = new NovelWriterPlugin(aiModel);
        var plugin = kernel.ImportPluginFromObject(novelPlugin);
       
        var summarizeChapterFunc = plugin["SummarizeChapter"];
        var summaryBuilder = new StringBuilder();
        var previousChapter = "none, 1st chapter";
        SendOutline?.Invoke(JsonSerializer.Serialize(chapters));
        foreach (var chapter in chapters)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var chapterCopy = chapter;
           
            var writeArgs = new KernelArguments()
            {
                ["outline"] = chapterCopy /*chapterExpanded*/,
                ["previousChapter"] = previousChapter,
                ["summary"] = summaryBuilder.ToString(),
                ["storyDescription"] = _description
            };
            var chapterText = "";
            

            await foreach (var token in WriteChapterStreaming(kernel, writeArgs, aiModel, cancellationToken))
            {
                yield return token;
                chapterText += token;
            }

            yield return "\n\n  ";
            
            previousChapter = chapterText;
            var summarizeArgs = new KernelArguments()
            {
                ["chapterText"] = chapterText
            };
            var summary = await summarizeChapterFunc.InvokeAsync<string>(summerizeKernel, summarizeArgs, cancellationToken);
            SendChapterText?.Invoke(this, new ChapterEventArgs(chapterText, summary!));
            summaryBuilder.AppendLine(summary);

        }
        
    }

    public async Task<string> WriteChapter(Kernel kernel, KernelArguments args, AIModel aiModel, CancellationToken cancellationToken)
    {
        var settings = GetWritingSettingsFromModel(aiModel);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var systemMessage = $"""
                             You are a creative and exciting fiction writer. You write intelligent, detailed and engrossing novels that expertly combine character development and growth, interpersonal and international intrigue, and thrilling action. You are tasked with writing a chapter for a novel. Ensure the chapter is engaging, cohesive, and well-structured. Your writing should always contain as much detail as possible. Always write with characters first, preferring natural sounding dialog over exposition. Most importantly, follow the provided **Writing Guide**.

                             ## Writing Style Guide

                             {Prompts.StyleGuide}

                             """;


        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var systemPrompt = await promptTemplateFactory.Create(new PromptTemplateConfig(Prompts.ChapterWriterPrompt)).RenderAsync(kernel, args, cancellationToken);
        var chatHistory = new ChatHistory(systemMessage);

        chatHistory.AddUserMessage(systemPrompt);

        var hasMore = false;
        var currentChapter = "";
        var response = await chat.GetChatMessageContentAsync(chatHistory, settings, kernel, cancellationToken);
        return response.Content!;
    }

   
    public async Task<string> RewriteChapter(string original, string feedback, AIModel model = AIModel.Gpt41)
    {
        var kernel = CreateKernel(model);
        var settings = GetWritingSettingsFromModel(model);
        var args = new KernelArguments(settings) { ["original"] = original, ["feedback"] = feedback };
        var newChapter = await kernel.InvokePromptAsync<string>(Prompts.ChapterRevisitPrompt, args);
        return newChapter!;
    }

    public Task<string?> ModifyOutlineSection(string? outline, string selectedText, string instructions,
        AIModel model = AIModel.Gpt41Mini)
    {
        var kernel = CreateKernel(model);
        var settings = GetOutlinePromptExecutionSettingsFromModel(model);
        var args = new KernelArguments(settings) { ["outline"] = outline, ["selectedSection"] = selectedText, ["instructions"] = instructions };
        var result = kernel.InvokePromptAsync<string>(Prompts.ModifyPartialOutlinePrompt, args);
        return result;
    }

    private async IAsyncEnumerable<string> WriteChapterStreaming(Kernel kernel, KernelArguments args, AIModel aiModel,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var settings = GetWritingSettingsFromModel(aiModel);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var systemMessage = $"""
                                You are a creative and exciting fiction writer. You write intelligent, detailed and engrossing novels that expertly combine character development and growth, interpersonal and international intrigue, and thrilling action. You are tasked with writing a chapter for a novel. Ensure the chapter is engaging, cohesive, and well-structured. Your writing should always contain as much detail as possible. Always write with characters first, preferring natural sounding dialog over exposition. Most importantly, follow the provided **Writing Guide**.
                                
                                ## Writing Style Guide
                                
                                {Prompts.StyleGuide}
                               
                                """;

        
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var writerPrompt = await promptTemplateFactory.Create(new PromptTemplateConfig(Prompts.ChapterWriterPrompt)).RenderAsync(kernel, args, cancellationToken);
        var chatHistory = new ChatHistory(systemMessage);
        
        chatHistory.AddUserMessage(writerPrompt);
        
        var currentChapter = "";
        await foreach (var message in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken))
        {
            var messageContent = message.Content!;
            currentChapter += messageContent;
            yield return messageContent;
        }

    }

    public async Task<Feedback> ProvideRewriteFeedback(ChapterOutline chapterOutline,
        AIModel aiModel = AIModel.GeminiFlash, string? additionalInstructions = null)
    {
        var kernel = CreateKernel(aiModel);
        var settings = GetRewriteSettingsFromModel(aiModel);
        var summary = string.Join("\n", appState.NovelInfo.ChapterOutlines.Select(x => x.Summary));
        var args = new KernelArguments(settings) { ["chapterText"] = chapterOutline.FullText, ["storySummary"] = summary, ["notes"] = additionalInstructions };
        var result = await kernel.InvokePromptAsync<string>(Prompts.ChapterImprovementPrompt, args);

        var formattedResult = result!.Replace("```json", "").Replace("```", "").Trim('\n');
        Console.WriteLine($"Feedback:\n--------------------------------------------\n{formattedResult}\n--------------------------------------------\n");
        var feedback = JsonSerializer.Deserialize<Feedback>(formattedResult);
        return feedback!;
    }
    public async Task<string> RewriteChapter(ChapterOutline chapterOutline, Feedback feedback, AIModel aiModel = AIModel.GeminiFlash, string? userNotes = null)
    {
        var kernel = CreateKernel(aiModel);
        //var promptFilter = new PromptFilter();
        //kernel.PromptRenderFilters.Add(promptFilter);
        var settings = GetWritingSettingsFromModel(aiModel);
        var args = new KernelArguments(settings) { ["chapterText"] = chapterOutline.FullText,["chapterOutline"] = chapterOutline.Text, ["strengths"] = feedback.Strengths, ["weaknesses"] = feedback.Weaknesses, ["suggestions"] = feedback.Suggestions, ["notes"] = userNotes };
        var func = KernelFunctionFactory.CreateFromPrompt(Prompts.ChapterRewritePrompt, functionName:"RewriteChapter");
        var result = "";
        await foreach (var message in func.InvokeStreamingAsync<string>(kernel, args))
        {
            result += message;
        }

        return result;
        //var rewrittenChapter = await kernel.InvokePromptAsync<string>(Prompts.ChapterRewritePrompt, args);

        //return rewrittenChapter;
    }
    public IAsyncEnumerable<string> ReviewFullNovel(NovelInfo novel, ReviewContext reviewContext,
        AIModel aiModel = AIModel.Gpt41)
    {
        var kernel = CreateKernel(aiModel);
        var promptTemplate = reviewContext == ReviewContext.None ? Prompts.NovelFullCoverageReviewPrompt : Prompts.NovelContextSpecificReviewPrompt;
        var reviewNovelFunc = KernelFunctionFactory.CreateFromPrompt(promptTemplate);
        var contextPrompt = reviewContext.GetPromptContext();

        var args = new KernelArguments() { ["novelText"] = novel.Text, ["context"] = contextPrompt };
        return reviewNovelFunc.InvokeStreamingAsync<string>(kernel, args);
    }

    public async Task<string> CompareTwoChapterVersions(string originalChapter, string revisedChapter,
        AIModel aiModel = AIModel.Gpt41)
    {
        var kernel = CreateKernel(aiModel);
        
        var promptConfig = new PromptTemplateConfig(Prompts.HeadToHeadEval)
        {
            InputVariables = [
                new() { Name = "versionA", AllowDangerouslySetContent = true },
                new() { Name = "versionB", AllowDangerouslySetContent = true }
            ]
        };
        var compareFunc = KernelFunctionFactory.CreateFromPrompt(promptConfig);
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var promptTemplate = promptTemplateFactory.Create(promptConfig);
        
        var args = new KernelArguments() { ["versionA"] = originalChapter, ["versionB"] = revisedChapter };
        var prompt = promptTemplate.RenderAsync(kernel, args);
        Console.WriteLine($"Prompt:\n {prompt}\n");
        return await compareFunc.InvokeAsync<string>(kernel, args) ?? string.Empty;
    }
    public async IAsyncEnumerable<string> ExecuteEditorAgentChat(ChatHistory chatHistory, AIModel aiModel = AIModel.Gpt41)
    {
        var kernel = CreateKernel(aiModel);
        kernel.ImportPluginFromType<EditorAgentPlugin>();
        var filter = new AutoFilter();
        kernel.AutoFunctionInvocationFilters.Add(filter);
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var template = new KernelPromptTemplateFactory().Create(new PromptTemplateConfig(Prompts.AgentPrompts.EditorAgentPrompt));
        var novelText = appState.NovelInfo.Text;
        var title = appState.NovelInfo.Title;
        var conceptDescription = appState.NovelInfo.ConceptDescription;
        var startIndex = conceptDescription.IndexOf("Characters:");
        var endIndex = conceptDescription.IndexOf("Primary Plot Events:");
		var characters = conceptDescription.Substring(startIndex, endIndex - startIndex);
        Console.WriteLine($"Characters: {characters}");
        var prompt = await template.RenderAsync(kernel, new KernelArguments() { ["novelText"] = novelText, ["title"] = title });
		var history = new ChatHistory(prompt);
		foreach (var message in chatHistory)
        {
            history.Add(message);
        }
        var settings = GetToolCallPromptExecutionSettings(aiModel);
        await foreach (var message in chat.GetStreamingChatMessageContentsAsync(history, settings, kernel))
        {
            yield return message.Content!;
        }
    }

    public async IAsyncEnumerable<string> ExecuteCharacterAgentChat(ChatHistory chatHistory, AIModel aiModel = AIModel.Grok3)
    {
		var kernel = CreateKernel(aiModel);
		kernel.ImportPluginFromType<EditorAgentPlugin>();
		var filter = new AutoFilter();
		kernel.AutoFunctionInvocationFilters.Add(filter);
		var chat = kernel.GetRequiredService<IChatCompletionService>();
		var template = new KernelPromptTemplateFactory().Create(new PromptTemplateConfig(Prompts.AgentPrompts.AsCharacterPrompt));
		var novelText = appState.NovelInfo.Text;
		var title = appState.NovelInfo.Title;
		var conceptDescription = appState.NovelInfo.ConceptDescription;
		var startIndex = conceptDescription.IndexOf("Characters:");
		var endIndex = conceptDescription.IndexOf("Plot ");
		var characters = conceptDescription.Substring(startIndex, endIndex - startIndex);
		Console.WriteLine($"Characters: {characters}");
		var prompt = await template.RenderAsync(kernel, new KernelArguments() { ["novelText"] = novelText, ["title"] = title, ["characters"] = characters });
		var history = new ChatHistory(prompt);
		foreach (var message in chatHistory)
		{
			history.Add(message);
		}
		var settings = GetToolCallPromptExecutionSettings(aiModel);
		await foreach (var message in chat.GetStreamingChatMessageContentsAsync(history, settings, kernel))
		{
			yield return message.Content!;
		}
	}

    private static PromptExecutionSettings GetWritingSettingsFromModel(AIModel model)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        if (model is AIModel.O3Mini or AIModel.Grok3Mini)
        {
            return new OpenAIPromptExecutionSettings { Store = true, MaxTokens = 34000, ReasoningEffort = "low" };
        }
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Store = true, Temperature = 0.9, MaxTokens = 8000 },
            "AnthropicAI" => new AmazonClaudeExecutionSettings(){MaxTokensToSample = 8192, Temperature = 0.9f},
            _ => new OpenAIPromptExecutionSettings { Store = true, Temperature = 0.9 }
        };
    }
    private static PromptExecutionSettings GetRewriteSettingsFromModel(AIModel model)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings {  },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "AnthropicAI" => new AmazonClaudeExecutionSettings() { MaxTokensToSample = 8192, Temperature = 0.9f },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Store = true, ChatSystemPrompt = "Provide feedback as a novel editor. Locate the flaws and provide notes for a re-write, if necessary. Use json format in response", Temperature = 0.8, ResponseFormat = "json_object"},
            _ => new OpenAIPromptExecutionSettings { Store = true, Temperature = 0.8 }
        };
    }

    public async IAsyncEnumerable<ReadOnlyMemory<byte>?> TextToAudioAsync(string text, string voice = "ash",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        
        AudioClient client = new("gpt-4o-mini-tts", configuration["OpenAI:ApiKey"]!);
        var splitText = text.SplitText(4096);
        foreach (var segment in splitText)
        {
            //var audioContent = await textToAudioService.GetAudioContentAsync(segment, executionSettings, kernel, cancellationToken: cancellationToken);
            //var audioData = audioContent.Data;
            var instructions = """
                               **Instructions for a Professional Audio Book Narrator**
                               
                               - **Delivery**:
                                 - Maintain a steady, measured pace that is neither rushed nor overly slow; allow the listener time to absorb the content, particularly during important passages.
                                 - Speak with consistent volume and clarity throughout; ensure every word is easy to understand without straining or mumbling.
                                 - Utilize natural pauses at punctuation marks and chapter breaks to maintain rhythm and clarity.
                                 - Employ subtle emphasis on key words, names, or turning points to guide the listener’s attention.
                                 - Vary pacing slightly during dialogue versus narration — slightly quicker for lively conversations, slightly slower for vivid descriptions or emotional moments, to enhance engagement.
                               
                               - **Voice**:
                                 - Use a clear, neutral adult voice (male or female, as specified by the publisher), displaying versatility for different characters if the text includes dialogue.
                                 - Strive for an expressive delivery; adopt distinctions for different characters through pitch, timbre, or accent shifts as necessary.
                                 - Exhibit vocal qualities of authority and warmth, imbuing the reading with professionalism and approachability.
                               
                               - **Tone**:
                                 - Maintain an overall tone that is polished, respectful, and attentive to the author’s intent—neither overly dramatic nor monotone.
                                 - Adjust tone subtly based on the narrative: more animated for scenes with action or excitement, and softer or more contemplative for reflective passages.
                                 - Avoid colloquial or overly casual delivery unless the text specifically calls for it.
                               
                               - **Pronunciation**:
                                 - Adhere to standard American or British English pronunciation as specified by the publisher or appropriate to the content.
                                 - Prioritize consistency, especially for recurring terms or character names.
                                 - If required by the text, implement regionally appropriate or culturally specific pronunciations.
                                 - Articulate clearly, avoiding regional accent drift unless indicated by the text or character dialogue.
                               """;
            // Fix for CS1503: Convert the string to ReadOnlyMemory<byte> using Encoding.UTF8.GetBytes
            BinaryContent requestContent = BinaryContent.Create(BinaryData.FromBytes(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                model = "gpt-4o-mini-tts",
                input = segment,
                voice = voice,
                instructions = instructions,
                speed = 1.25f
            })))));
            Console.WriteLine($"Starting TTS for part {splitText.IndexOf(segment) + 1} of {splitText.Count}");
            var speech = await client.GenerateSpeechAsync(requestContent);
            ReadOnlyMemory<byte> audioData = speech.GetRawResponse().Content; 
            yield return audioData;
            Console.WriteLine($"Completed TTS for part {splitText.IndexOf(segment) + 1} of {splitText.Count}");
            //SendAudioResponse?.Invoke(this, audioData);
        }

    }
    public static List<string> SplitMarkdownByHeaders(string markdownText)
    {
        var result = new List<string>();
        var headerPattern = new Regex(@"^(## .+)$", RegexOptions.Multiline);

        var matches = headerPattern.Matches(markdownText);

        var lastIndex = 0;
        foreach (Match match in matches)
        {
            if (lastIndex != match.Index)
            {
                if (lastIndex != 0)
                {
                    var segment = markdownText.Substring(lastIndex, match.Index - lastIndex).Trim();
                    result.Add(segment);
                }
                else
                {
                    // Capture the first header and content if the first header is at the start
                    var firstSegment = markdownText[..match.Index].Trim();
                    if (!string.IsNullOrEmpty(firstSegment))
                    {
                        result.Add(firstSegment);
                    }
                }
            }
            lastIndex = match.Index;
        }

        if (lastIndex == 0) return result;
        var lastSegment = markdownText[lastIndex..].Trim();
        result.Add(lastSegment);

        return result;
    }
    private readonly FunctionFilter _functionFilter = new();
    public Kernel CreateKernel(AIModel aiModel = AIModel.Gpt4OMini)
    {
        var kernelBuilder = Kernel.CreateBuilder();
       
        kernelBuilder.Services.AddSingleton(configuration);
        kernelBuilder.Services.AddSingleton(appState);
        AddDefaultKernelServices(aiModel, kernelBuilder);
        var kernel = kernelBuilder
            .Build();
        kernel.FunctionInvocationFilters.Add(_functionFilter);
        return kernel;

    }

    public static void AddDefaultKernelServices(AIModel aiModel, IKernelBuilder kernelBuilder)
    {
	    kernelBuilder.Services.AddLogging(builder =>
	    {
		    builder.AddConsole();
	    });
	    kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
	    {
		    c.AddStandardResilienceHandler().Configure(o =>
		    {
			    o.Retry.ShouldHandle = RetryShouldHandle;
			    o.Retry.BackoffType = DelayBackoffType.Exponential;
			    o.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(5) };
			    o.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
			    o.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(15) };
                
		    });
	    });
	    var providor = aiModel.GetModelProvidors().FirstOrDefault();
	    Console.WriteLine($"AI: {providor}, {aiModel.GetAIModelName()}");
        var client = DelegateHandlerFactory.GetHttpClientWithHandler<SystemToDeveloperRoleHandler>(_loggerFactory);
        var logClient = DelegateHandlerFactory.GetHttpClientWithHandler<LoggingHandler>(_loggerFactory);
        if (providor == "OpenAI" && (aiModel is AIModel.O3Mini))
            kernelBuilder.AddOpenAIChatCompletion(aiModel.GetAIModelName(), ConfigurationSettings.OpenAI!.ApiKey!, httpClient: client);
        else if (providor == "OpenAI")
		    kernelBuilder.AddOpenAIChatCompletion(aiModel.GetAIModelName(), ConfigurationSettings.OpenAI!.ApiKey!);
	    if (providor == "GoogleAI")
		    kernelBuilder.AddGoogleAIGeminiChatCompletion(aiModel.GetAIModelName(), ConfigurationSettings.GoogleAI!.ApiKey!, httpClient:logClient);
        if (providor == "GrokAI")
            kernelBuilder.AddOpenAIChatCompletion(aiModel.GetAIModelName(),
                apiKey: ConfigurationSettings.GrokAI!.ApiKey!, endpoint: new Uri("https://api.x.ai/v1"));
	    if (providor == "MistralAI")
		    kernelBuilder.AddMistralChatCompletion(aiModel.GetAIModelName(), ConfigurationSettings.MistralAI!.ApiKey!);
        if (providor == "AnthropicAI")
            kernelBuilder.AddBedrockChatCompletionService(aiModel.GetAIModelName());
#if DEBUG
        if (aiModel == AIModel.LocalModel)
            kernelBuilder.AddOpenAIChatCompletion(aiModel.GetAIModelName(), apiKey: "", endpoint: new Uri("http://localhost:1234/v1"));
#endif

    }

    private static ValueTask<bool> RetryShouldHandle(RetryPredicateArguments<HttpResponseMessage> args)
    {
        switch (args.Outcome.Result?.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
                Console.WriteLine("Too Many Requests --> Retry");
                return ValueTask.FromResult(true);
            case HttpStatusCode.ServiceUnavailable:
                Console.WriteLine("Service Unavailable --> Retry");
                return ValueTask.FromResult(true);
            case HttpStatusCode.InternalServerError:
                Console.WriteLine("Internal Server Error --> Retry");
                return ValueTask.FromResult(true);
            case HttpStatusCode.GatewayTimeout:
                Console.WriteLine("Gateway Timeout --> Retry");
                return ValueTask.FromResult(true);
            case HttpStatusCode.RequestTimeout:
                Console.WriteLine("Request Timeout --> Retry");
                return ValueTask.FromResult(true);
            case HttpStatusCode.BadGateway:
                Console.WriteLine("Bad Gateway --> Retry");
                return ValueTask.FromResult(true);
            case HttpStatusCode.BadRequest:
                Console.WriteLine("Bad Request --> Retry");
                return ValueTask.FromResult(true);
            default:
                return ValueTask.FromResult(false);
                
        }
    }

    private PromptExecutionSettings GetPromptExecutionSettingsFromModel(AIModel model, double tempurature, int maxTokens = 1024, bool isJson = true)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { ResponseMimeType = "application/json", ResponseSchema = typeof(NovelConceptOutput), /*MaxTokens = maxTokens,*/ Temperature = tempurature },
            "MistralAI" => new MistralAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature, ExtensionData = new Dictionary<string, object> { ["response_format"] = new { type = "json_object" } }, },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Store = true, MaxTokens = maxTokens, Temperature = tempurature, ResponseFormat = typeof(NovelConceptOutput) },
            "AnthropicAI" => new AmazonClaudeExecutionSettings { MaxTokensToSample = 8192 },
            _ => new OpenAIPromptExecutionSettings { Store = true, MaxTokens = maxTokens, Temperature = tempurature }
        };
    }
    private PromptExecutionSettings GetOutlinePromptExecutionSettingsFromModel(AIModel model)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        if (model is AIModel.O3Mini)
        {
            
            return new OpenAIPromptExecutionSettings { Store = true, MaxTokens = 34000, ReasoningEffort = "low" };
        }
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Store = true, MaxTokens = 14000},
            "AnthropicAI" => new AmazonClaudeExecutionSettings { MaxTokensToSample = 8192 },
            _ => new OpenAIPromptExecutionSettings { Store = true,  }
        };
    }
    private PromptExecutionSettings GetToolCallPromptExecutionSettings(AIModel model)
    {
        return new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        //var providor = model.GetModelProvidors().FirstOrDefault();
        //return providor switch
        //{
        //    "GoogleAI" => new GeminiPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions},
        //    "MistralAI" => new MistralAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ToolCallBehavior = MistralAIToolCallBehavior.AutoInvokeKernelFunctions},
        //    "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Store = true, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions},
        //    _ => new OpenAIPromptExecutionSettings { Store = true, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions }
        //};
    }
}