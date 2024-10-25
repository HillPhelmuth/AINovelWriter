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
using Microsoft.SemanticKernel.TextToImage;


namespace AINovelWriter.Shared.Services;

public class FineTuneLine
{
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}
public class NovelWriterService(IConfiguration configuration, AppState appState) : INovelWriter
{
    public event Action<string>? SendOutline;
    public event EventHandler<ChapterEventArgs>? SendChapterText;
    public event EventHandler<string>? TextToImageUrl;
    public event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
    public event EventHandler<AudioState>? SendAudioStateUpdate;

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
    public async Task<NovelConcepts> GenerateNovelIdea(GenreCategoryItem genre, List<Genre> subgenres, NovelLength length)
    {
        var random = new Random();
        var roll = random.Next(1, 7);
        var aIModel = roll switch
        {
            1 => AIModel.Gpt35Turbo,
            2 => AIModel.Gpt4Mini,
            3 => AIModel.GeminiFlash,
            4 => AIModel.Gpt4O,
            5 => AIModel.OpenMistralNemo,
            6 => AIModel.Gemini15,
            _ => AIModel.Gemini10
        };
        var kernel = CreateKernel(aIModel);
        var rng = new Random();
        var personality = Enum.GetValues<Personality>()[rng.Next(Enum.GetValues<Personality>().Length)];
        
        var personality2 = Enum.GetValues<Personality>()[rng.Next(Enum.GetValues<Personality>().Length)];
        var personalityVar =
            $"**{personality.ToString()} ** as in {personality.GetDescription()} or **{personality2.ToString()}** as in {personality2.GetDescription()}.";
        var lengthDescription = length.GetDescription();
        string Prompt = $$$"""
                              You are a creative novel idea generator. Your task is to assist the user in developing a unique and captivating novel idea.
                              
                              You will receive a **Genre** and **Sub-genre**. Use them to shape the **Theme** of the story. The generated idea must include the following components:  
                              
                              - **Title**: Use the provided title if available, otherwise generate one.  
                              - **Theme**: Use the given theme if provided, otherwise create one. Ensure the theme reflects the genre and sub-genre.  
                              - **Character Details**: Incorporate any provided character details; otherwise, create compelling characters.  
                              - **Key Plot Events**: Include any plot events provided; otherwise, generate significant events to drive the narrative.
                              
                              ### Audience:  
                              This novel should resonate with readers who are **{{ $personalities }}**.  
                              
                              ### Length:  
                              The content should align with the following length description (longer novels will require more characters and plot events): **{{ $lengthDescription }}**.  
                              
                              ### Required Output Format:  
                              Generate your response in the following JSON structure. Modify only empty properties:
                              
                              ```json
                              {
                                 "Title": "Title of the Novel",
                                 "Theme": "Theme of the Novel described in 1-3 sentances",
                                 "Characters": "3 - 7 main Character Details (Short novels should get 3, long or epic should get 7)",
                                 "PlotEvents": "3 - 10 primary Plot Events (short novels should get 3-4, medium 5-6, long 7-8, epic 9-10)"
                              }
                              ```
                              
                              ### Genre:  
                              **{{ $genre }}**  
                              
                              ### Sub-genres:  
                              **{{ $subgenre }}**
                              
                              """;
       
        var settings = GetPromptExecutionSettingsFromModel(aIModel, 0.9);
        var subgenreString = string.Join("\n", subgenres.Select(x => x.ToString()));
        Console.WriteLine($"Subgenres: \n---------------------\n{subgenreString}\n------------------------------------\n");
        var args = new KernelArguments(settings) { ["genre"] = $"{genre.Name}\n{genre.Description}", ["subgenre"] = subgenreString, ["personalities"] = personalityVar, ["lengthDescription"] = lengthDescription };
        var json = await kernel.InvokePromptAsync<string>(Prompt, args);
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
            concepts = JsonSerializer.Deserialize<NovelConceptFailover>(rawJson)?.AsNovelConcepts();
        }

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
        SendOutline?.Invoke(JsonSerializer.Serialize(chapters));
        foreach (var chapter in chapters)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var chapterCopy = chapter;
            
            var writeArgs = new KernelArguments()
            {
                ["outline"] = chapterCopy/*chapterExpanded*/,
                ["previousChapter"] = previousChapter,
                ["summary"] = summaryBuilder.ToString(),
                ["storyDescription"] = _description
            };
            var chapterText = "";
            

            await foreach (var token in WriteChapterStreaming(kernel, writeArgs, aiModel, chapterCopy, cancellationToken))
            {
                //SendTextResponse?.Invoke(this, token);
                yield return token;
                chapterText += token;
            }

            //SendTitle?.Invoke(this, "\n\n  ");
            yield return "\n\n  ";
            
            previousChapter = chapterText;
            var summarizeArgs = new KernelArguments()
            {
                ["chapterText"] = chapterText
            };
            var summary = await summarizeChapterFunc.InvokeAsync<string>(kernel, summarizeArgs, cancellationToken);
            SendChapterText?.Invoke(this, new ChapterEventArgs(chapterText, summary!));
            summaryBuilder.AppendLine(summary);

        }
        //var image = await TextToImage(outline);
        //TextToImageUrl?.Invoke(this, image);
    }
    private async IAsyncEnumerable<string> WriteChapterStreaming(Kernel kernel, KernelArguments args, AIModel aiModel,
        string outline, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        await foreach (var message in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken))
        {
            var messageContent = message.Content!;
            currentChapter += messageContent;
            yield return messageContent;
            if (aiModel != AIModel.Gpt4O) continue;
            if (message.Metadata?["FinishReason"] is not ChatFinishReason item) continue;
            Console.WriteLine($"Finish Reason: {(ChatFinishReason?)item}");
            hasMore = item == ChatFinishReason.Length;
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
        AIModel aiModel = AIModel.Gpt4O)
    {
        var kernel = CreateKernel(aiModel);
        var promptTemplate = reviewContext == ReviewContext.None ? Prompts.NovelFullCoverageReviewPrompt : Prompts.NovelContextSpecificReviewPrompt;
        var reviewNovelFunc = KernelFunctionFactory.CreateFromPrompt(promptTemplate);
        var contextPrompt = reviewContext.GetPromptContext();

        var args = new KernelArguments() { ["novelText"] = novel.Text, ["context"] = contextPrompt };
        return reviewNovelFunc.InvokeStreamingAsync<string>(kernel, args);
    }

    
    public async IAsyncEnumerable<string> ExecuteEditorAgentChat(ChatHistory chatHistory, AIModel aiModel = AIModel.Gpt4O)
    {
        var kernel = CreateKernel(aiModel);
        kernel.ImportPluginFromType<EditorAgentPlugin>();
        var filter = new AutoFilter();
        kernel.AutoFunctionInvocationFilters.Add(filter);
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory(Prompts.AgentPrompts.EditorAgentPrompt);
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
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings { },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Temperature = 0.9, MaxTokens = 8000, ExtensionData = new Dictionary<string, object> { ["store"] =true }, },
            _ => new OpenAIPromptExecutionSettings { Temperature = 0.8 }
        };
    }
    private static PromptExecutionSettings GetRewriteSettingsFromModel(AIModel model)
    {
        var providor = model.GetModelProvidors().FirstOrDefault();
        return providor switch
        {
            "GoogleAI" => new GeminiPromptExecutionSettings {  },
            "MistralAI" => new MistralAIPromptExecutionSettings { },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { ChatSystemPrompt = "Provide feedback as a novel editor. Locate the flaws and provide notes for a re-write, if necessary. Use json format in response", Temperature = 0.8, ResponseFormat = "json_object"},
            _ => new OpenAIPromptExecutionSettings { Temperature = 0.8 }
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
        kernelBuilder.Services.AddSingleton(appState);
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
        kernelBuilder.Services.AddSingleton(appState);
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
        Console.WriteLine($"AI: {providor}");
        if (providor.Contains("OpenAI"))
            kernelBuilder.AddOpenAIChatCompletion(aiModel.GetOpenAIModelName(), configuration["OpenAI:ApiKey"]!);
        if (providor == "GoogleAI")
            kernelBuilder.AddGoogleAIGeminiChatCompletion(aiModel.GetOpenAIModelName(), configuration["GoogleAI:ApiKey"]!);
        if (providor == "MistralAI")
            kernelBuilder.AddMistralChatCompletion(aiModel.GetOpenAIModelName(), configuration["MistralAI:ApiKey"]!);
        if (aiModel == AIModel.LocalModel)
            kernelBuilder.AddOpenAIChatCompletion(aiModel.GetOpenAIModelName(), apiKey: "", endpoint: new Uri("http://localhost:1234/v1"));
        return kernelBuilder
            .Build();

    }

    private ValueTask<bool> RetryShouldHandle(RetryPredicateArguments<HttpResponseMessage> args)
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
            "GoogleAI" => new GeminiPromptExecutionSettings { ExtensionData = new Dictionary<string, object> { ["responseMimeType"] = "application/json" }, MaxTokens = maxTokens, Temperature = tempurature },
            "MistralAI" => new MistralAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature, ExtensionData = new Dictionary<string, object> { ["response_format"] = new { type = "json_object" } }, },
            "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature, ResponseFormat = "json_object" },
            _ => new OpenAIPromptExecutionSettings { MaxTokens = maxTokens, Temperature = tempurature }
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
        //    "OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions},
        //    _ => new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions }
        //};
    }
}