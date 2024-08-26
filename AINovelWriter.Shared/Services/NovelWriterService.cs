using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AINovelWriter.Evals;
using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Plugins;
using Azure.AI.OpenAI;
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
using Microsoft.SemanticKernel.TextToImage;
using Polly;
using PromptFlowEvalsAsPlugins;

namespace AINovelWriter.Shared.Services;

public class NovelWriterService(IConfiguration configuration) : INovelWriter
{
    public event Action<string>? SendChapters;
    public event EventHandler<string>? SendChapterText;
    public event EventHandler<string>? TextToImageUrl;
    public event EventHandler<string>? SendTitle;
    public event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
    public event EventHandler<AudioState>? SendAudioStateUpdate;

    public async Task<NovelInfo> ReverseEngineerNovel(string directoryPath, int volume, string title)
	{
		var chapters = ReverseWriterService.ParseNovelChapters(directoryPath).Where(x => x.Volume == volume).ToList();
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
			chapterOuts.Add(new ChapterOutline($"Chapter: 00{chapter.Part}-{chapter.Chapter}", chapter.Outline){FullText = chapter.Text});
		}
        var novelInfo = new NovelInfo
        {
            Outline = string.Join("\n", chapters.Select(x => x.Outline)),
            Text = string.Join("\n", chapters.Select(x => x.Text)), 
            ChapterOutlines = chapterOuts,
            Title = title
        };
        const string Prompt = """
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
        var json = await kernel.InvokePromptAsync<string>(Prompt, novelArgs);
        //var concepts = JsonSerializer.Deserialize<NovelConcepts>(json.Replace("```json", "").Replace("```", "").Trim('\n'));
        //concepts.Title = title;
        novelInfo.ConceptDescription =json;
        //var args = new KernelArguments() { ["novel_chapter"] = JsonSerializer.Serialize(chapters) };
        //var novelInfo = await reverseEngineerFunc.InvokeAsync<NovelInfo>(kernel, args);
        return novelInfo;
	}
	public async Task<NovelConcepts> GenerateNovelIdea(GenreCategoryItem genre, List<Genre> subgenres)
    {
        var random = new Random();
        var roll = random.Next(1, 7);
        AIModel aIModel = roll switch
        {
            1 => AIModel.Gpt35Turbo,
            2 => AIModel.Gpt4Mini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt4O,
            5 => AIModel.OpenMistral7B,
            6 => AIModel.OpenMixtral8By7B,
            _ => AIModel.Gemini10
        };
        var kernel = CreateKernel(aIModel);
        const string Prompt = """
                              You are a novel idea generator. Provided a Genre and a Sub-genre, generate a novel idea.
                              The idea should contain a Title, a Theme, a few Character Details, and a few key Plot Events.
                              ## Output
                              Your output must be in json using the following format:
                              ```json
                              {
                              	"Title": "Title of the Novel",
                              	"Theme": "Theme of the Novel described in 1-3 sentances",
                              	"Characters": "3 - 5 main Character Details",
                              	"PlotEvents": "3 - 5 primary Plot Events"
                              }
                              ```
                              Include the Genre and Sub-genres in the Theme.
                              ## Genre
                              {{ $genre }}
                              
                              ## Sub-genres
                              {{ $subgenre }}
                              """;
        var settings = GetPromptExecutionSettingsFromModel(aIModel, 0.9);
        var subgenreString = string.Join("\n", subgenres.Select(x => x.ToString()));
        Console.WriteLine($"Subgenres: \n---------------------\n{subgenreString}\n------------------------------------\n");
        var args = new KernelArguments(settings) { ["genre"] = $"{genre.Name}\n{genre.Description}", ["subgenre"] = subgenreString };
        var json = await kernel.InvokePromptAsync<string>(Prompt, args);
        Console.WriteLine($"Novel Idea:\n------------------------------------\n {json}\n------------------------------------\n");
        var concepts = JsonSerializer.Deserialize<NovelConcepts>(json.Replace("```json", "").Replace("```", "").Trim('\n'));
        concepts.Genre = genre.Category;
        concepts.SubGenres = subgenres;
        return concepts;

    }
    public async Task<string> CreateNovelOutline(string theme, string characterDetails = "", string plotEvents = "",
        string novelTitle = "", int chapters = 15, AIModel aIModel = AIModel.Gpt4O)
    {
        var kernel = CreateKernel(aIModel);
        var novelWriter = new NovelWriterPlugin(aIModel);
        var plugin = kernel.ImportPluginFromObject(novelWriter);
        var createOutlineFunc = plugin["CreateNovelOutline"];
        _title = novelTitle;
        var args = new KernelArguments()
        {
            ["theme"] = theme,
            ["characterDetails"] = characterDetails,
            ["plotEvents"] = plotEvents,
            ["novelTitle"] = novelTitle,
            ["chapters"] = chapters
        };
        var outline = await createOutlineFunc.InvokeAsync<string>(kernel, args);
        _description =
            $"""
             Theme or topic:
             {theme}

             Character Details:
             {characterDetails}

             Plot Events:
             {plotEvents}
             """;
        //AdditionalAgentText?.Invoke($"<p>{outline}</p>");
        return outline;
    }
    private string _description = "";
    private string _title = "";

    public async IAsyncEnumerable<string> WriteNovel(string outline, string authorStyle = "", AIModel aiModel = AIModel.Gpt4O,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        //var chapterOutline = JsonSerializer.Deserialize<ChapterOutline>(outline.Replace("```json","").Replace("```","").Trim('\n'));
        var chapters = SplitMarkdownByHeaders(outline);
        var kernel = CreateKernel(aiModel);
        var novelPlugin = new NovelWriterPlugin(aiModel);
        var plugin = kernel.ImportPluginFromObject(novelPlugin);

        var initialContext = new KernelArguments()
        {
            ["chapters"] = outline,
            ["description"] = _description,
            ["title"] = _title
        };
        var writeChapterFunc = plugin["WriteChapterStreaming"];
        var expandChapterFunc = plugin["ExpandChapterOutline"];
        var summarizeChapterFunc = plugin["SummarizeChapter"];
        var summaryBuilder = new StringBuilder();
        var previousChapter = "none, 1st chapter";
        SendChapters?.Invoke(JsonSerializer.Serialize(chapters));
        foreach (var chapter in chapters)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var chapterCopy = chapter/* + "\n\n**expected length:** 100 paragraphs"*/;
            //var flashKernel = CreateKernel(AIModel.Gpt4Mini);
            //var chapterExpanded = await expandChapterFunc.InvokeAsync<string>(flashKernel, new KernelArguments() { ["outline"] = chapterCopy, ["storyDescription"] = _description }, cancellationToken);
            var writeArgs = new KernelArguments()
            {
                ["outline"] = chapterCopy/*chapterExpanded*/,
                ["previousChapter"] = previousChapter,
                ["summary"] = summaryBuilder.ToString(),
                ["storyDescription"] = _description
            };
            var chapterText = "";
            //yield return $"[CHAPTER] {chapter.Split('\n')[0]}";

            await foreach (var token in WriteChapterStreaming(kernel, writeArgs, aiModel, authorStyle, cancellationToken))
            {
                //SendTextResponse?.Invoke(this, token);
                yield return token;
                chapterText += token;
            }
            SendChapterText?.Invoke(this, chapterText);
            //SendTitle?.Invoke(this, "\n\n  ");
            yield return "\n\n  ";
            //AdditionalAgentText?.Invoke($"<p>{chapterText}</p>");
            previousChapter = chapterText;
            var summarizeArgs = new KernelArguments()
            {
                ["chapterText"] = chapterText
            };
            var summary = await summarizeChapterFunc.InvokeAsync<string>(kernel, summarizeArgs, cancellationToken);
            summaryBuilder.AppendLine(chapterText);

        }
        //var image = await TextToImage(outline);
        //TextToImageUrl?.Invoke(this, image);
    }
    public async IAsyncEnumerable<string> WriteChapterStreaming(Kernel kernel, KernelArguments args, AIModel aiModel, string authorStyle = "", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var settings = GetWritingSettingsFromModel(aiModel);
        
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var systemMessage = $"""
                             You are a creative and exciting fiction writer. You write intelligent, detailed and engrossing novels that expertly combine character development and growth, interpersonal and international intrigue, and thrilling action. You are tasked with writing a chapter for a novel. Ensure the chapter is engaging, cohesive, and well-structured. Your writing should always contain as much detail as possible. Always write with characters first, preferring dialog over exposition. Most importantly, follow the provided Writing Guide.
                            
                             """;

        var chatHistory = new ChatHistory(systemMessage);
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var userPrompt = await promptTemplateFactory.Create(new PromptTemplateConfig(Prompts.ChapterWriterPrompt)).RenderAsync(kernel, args, cancellationToken);
        chatHistory.AddUserMessage(userPrompt);
        //Azure.AI.OpenAI.StreamingChatCompletionsUpdate
        var hasMore = false;
        var currentChapter = "";
        await foreach (var message in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken))
        {
            var messageContent = message.Content!;
            currentChapter += messageContent;
            yield return messageContent;
            if (aiModel != AIModel.Gpt4O) continue;
            var item = message.Metadata?["FinishReason"] as CompletionsFinishReason?;
            if (item == null) continue;
            Console.WriteLine($"Finish Reason: {item}");
            hasMore = item.Value == CompletionsFinishReason.TokenLimitReached;
        }
        if (!hasMore) yield break;
        chatHistory.AddAssistantMessage(currentChapter);
        chatHistory.AddUserMessage("Continue the chapter");
        await foreach (var message in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken))
        {
            yield return message.Content!;
        }
        //return kernel.InvokePromptStreamingAsync<string>(ChapterWriterPrompt, args);

    }

    public async Task<List<ResultScore>> ExecuteChapterEval(string chapterText, string details)
    {
        var kernel = CreateKernel();
        var novelEvalService = new NovelEvalService(kernel);
        var inputs = novelEvalService.CreateInputModels(chapterText, details);
        var resultScores = await novelEvalService.ExecuteEvals(inputs);
        return resultScores;
    }

    public async Task<Feedback> ProvideRewriteFeedback(string chapterText, AIModel aiModel = AIModel.GeminiFlash, string? additionalInstructions = null)
    {
        var kernel = CreateKernel(aiModel);
        var settings = GetRewriteSettingsFromModel(aiModel);
        var args = new KernelArguments(settings) { ["chapterText"] = chapterText, ["notes"] = additionalInstructions };
        var result = await kernel.InvokePromptAsync<string>(Prompts.ChapterImprovementPrompt, args);
        
        var formattedResult = result!.Replace("```json", "").Replace("```", "").Trim('\n');
        Console.WriteLine($"Feedback:\n--------------------------------------------\n{formattedResult}\n--------------------------------------------\n");
		var feedback = JsonSerializer.Deserialize<Feedback>(formattedResult);
        return feedback!;
    }
    public async Task<string> RewriteChapter(ChapterOutline chapterOutline, Feedback feedback,
        AIModel aiModel = AIModel.GeminiFlash)
    {
        var kernel = CreateKernel(aiModel);
        var promptFilter = new PromptFilter();
        kernel.PromptRenderFilters.Add(promptFilter);
        var settings = GetWritingSettingsFromModel(aiModel);
        var args = new KernelArguments(settings) { ["chapterText"] = chapterOutline.FullText, ["strengths"] = feedback.Strengths, ["weaknesses"] = feedback.Weaknesses, ["suggestions"] = feedback.Suggestions };
        var rewrittenChapter = await kernel.InvokePromptAsync<string>(Prompts.ChapterRewritePrompt, args);

        return rewrittenChapter;
    }

    private static PromptExecutionSettings GetWritingSettingsFromModel(AIModel model)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { ChatSystemPrompt = "You are a professional novelist tasked with writing a chapter for a novel. Follow the writing guide.  Ensure the chapter is very detailed, engaging, cohesive, and well-structured.", Temperature = 0.8, MaxTokens = 6000 },
            _ => new OpenAIPromptExecutionSettings { Temperature = 0.8 }
        };
    }
    private static PromptExecutionSettings GetRewriteSettingsFromModel(AIModel model)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { ChatSystemPrompt = "Provide feedback as a novel editor. Locate the flaws and provide notes for a re-write, if necessary. Use json format in response", Temperature = 0.7 },
            _ => new OpenAIPromptExecutionSettings { Temperature = 0.7 }
        };
    }
    public async Task<string> TextToImage(string novelOutline, string imageStyle = "photo-realistic")
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        kernelBuilder.Services.AddSingleton(configuration);
        kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler().Configure(o =>
            {
                o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                o.Retry.BackoffType = DelayBackoffType.Exponential;
                o.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(90) };
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(180);
                o.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(5) };
            });
        });
        var kernel = kernelBuilder
            .AddOpenAITextToImage(configuration["OpenAI:ApiKey"]!, modelId: "dall-e-3")
            .AddOpenAIChatCompletion("gpt-4o-2024-08-06", configuration["OpenAI:ApiKey"]!)
            .Build();
        var imagePromptPrompt =
                            """
                            Generate a prompt for an image generating model to create a {{$imageStyle}} image for the cover of this novel. Your response will be passed directly to the image generating model, so do not include any additional text.
                            Ensure that the art style is {{ $imageStyle}} so emphasize it in your prompt.
                            ## Novel Outline
                            {{ $novel }}

                            ## Output
                            Limit your prompt to 200 words.
                            """;

        var kernelArguments = new KernelArguments { ["novel"] = novelOutline, ["imageStyle"] = imageStyle };
        var prompt = await kernel.InvokePromptAsync<string>(imagePromptPrompt, kernelArguments);
        Console.WriteLine($"ImageStyle: {imageStyle}\n\nPROMPT:\n\n____________{prompt}\n\n________________\n\n");
        var imageService = kernel.GetRequiredService<OpenAITextToImageService>();
        var imageContent = await imageService.GenerateImageAsync(prompt + $"\n\n{imageStyle}", 1024, 1024);
        return imageContent;

    }
    public async IAsyncEnumerable<ReadOnlyMemory<byte>?> TextToAudioAsync(string text, string voice = "onyx",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        kernelBuilder.Services.AddSingleton(configuration);
        kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler().Configure(o =>
            {
                o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                o.Retry.BackoffType = DelayBackoffType.Exponential;
                o.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(90) };
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(180);
                o.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(5) };
            });
        });
        var kernel = kernelBuilder
            .AddOpenAITextToAudio(
                modelId: "tts-1",
                apiKey: configuration["OpenAI:ApiKey"]!)
            .Build();

        var textToAudioService = kernel.GetRequiredService<ITextToAudioService>();

        OpenAITextToAudioExecutionSettings executionSettings = new()
        {
            Voice = voice, // The voice to use when generating the audio.
            ResponseFormat = "mp3", // The format to audio in.
            Speed = 1.0f // The speed of the generated audio.
        };
        foreach (var segment in text.SplitText(4096))
        {
            var audioContent = await textToAudioService.GetAudioContentAsync(segment, executionSettings, kernel, cancellationToken: cancellationToken);
            var audioData = audioContent.Data;
            yield return audioData;
            //SendAudioResponse?.Invoke(this, audioData);
        }
        // Convert text to audio

        // Save audio content to a file
        // await File.WriteAllBytesAsync(AudioFilePath, audioContent.Data!.ToArray());
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
    public Kernel CreateKernel(AIModel aiModel = AIModel.Gpt4Mini)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        kernelBuilder.Services.AddSingleton(configuration);
        kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler().Configure(o =>
            {
                o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                o.Retry.BackoffType = DelayBackoffType.Exponential;
                o.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(180) };
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(360);
                o.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(5) };
            });
        });
        var providor = aiModel.GetModelProvidors().FirstOrDefault();
        Console.WriteLine($"AI: {providor}");
        if (providor.Contains("OpenAI"))
            kernelBuilder.AddOpenAIChatCompletion(aiModel.GetOpenAIModelName(), configuration["OpenAI:ApiKey"]!);
        if (providor == "GoogleAI")
            kernelBuilder.AddGoogleAIGeminiChatCompletion(aiModel.GetOpenAIModelName(), configuration["GoogleAI:ApiKey"]!);
        if (providor == "MistralAI")
            kernelBuilder.AddMistralChatCompletion(aiModel.GetOpenAIModelName(), configuration["MistralAI:ApiKey"]!);
        return kernelBuilder
            .Build();

    }
    private PromptExecutionSettings GetPromptExecutionSettingsFromModel(AIModel model, double tempurature, int maxTokens = 1024, bool isJson = true)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { ExtensionData = new Dictionary<string, object> { ["responseMimeType"] = "application/json" }, MaxTokens = maxTokens, Temperature = tempurature },
            "MistralAI" => new MistralAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature, ExtensionData = new Dictionary<string, object> { ["response_format"] = new {type = "json_object" } }, },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature, ResponseFormat = "json_object" },
            _ => new OpenAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature }
        };
    }
}