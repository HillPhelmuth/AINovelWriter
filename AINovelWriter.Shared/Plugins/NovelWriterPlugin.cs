using AINovelWriter.Shared.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using AINovelWriter.Shared.Services;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Amazon;

namespace AINovelWriter.Shared.Plugins;

public class NovelWriterPlugin(AIModel aIModel = AIModel.Gpt41)
{
    public const string SummaryPrompt = "Summarize the following chapter, focusing on character development, plot progression, and important details. Include specific actions, emotions, and relationships that are revealed. Pay attention to any shifts in character motivations or relationships. The summary should be detailed and insightful, providing more than just a basic overview of the events. \n\n```\n {{$novel_chapter}} \n ```";
    private AIModel _aIModel = aIModel;

	[KernelFunction, Description("Write a chapter of a novel based on the provided outline, previous chapter, and summary of the full novel so far")]
	public async IAsyncEnumerable<string> WriteChapterStreaming(Kernel kernel, [Description("Chapter Outline")] string outline, [Description("Story description and characters")] string storyDescription, [Description("Precise text of the previous chapter")] string previousChapter, [Description("Combined summary of each chapter so far")] string summary)
	{
		var settings = GetPromptExecutionSettingsFromModel(_aIModel);
		var args = new KernelArguments(settings)
		{
			["chapter_outline"] = outline,
			["description"] = storyDescription,
			["previous_chapter"] = previousChapter,
			["summary"] = summary
		};
		var chat = kernel.GetRequiredService<IChatCompletionService>();
		var chatHistory = new ChatHistory("You are a professional novelist tasked with writing a chapter for a novel. The chapter should be exactly 3000 words. It is very important to maintain this exact word count. Ensure the chapter is engaging, cohesive, and well-structured.");
		var promptTemplateFactory = new KernelPromptTemplateFactory();
		var userPrompt = await promptTemplateFactory.Create(new PromptTemplateConfig(Prompts.ChapterWriterPrompt)).RenderAsync(kernel, args);
		chatHistory.AddUserMessage(userPrompt);
		await foreach (var message in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
		{
			yield return message.Content!;
		}
		//return kernel.InvokePromptStreamingAsync<string>(ChapterWriterPrompt, args);

	}
	[KernelFunction, Description("Write a chapter of a novel based on the provided outline, previous chapter, and summary of the full novel so far")]
	public async Task<string> WriteChapter(Kernel kernelIn, [Description("Chapter Outline")] string outline, [Description("Story description and characters")] string storyDescription, [Description("Precise text of the previous chapter")] string previousChapter, [Description("Combined summary of each chapter so far")] string summary)
	{
		var settings = GetPromptExecutionSettingsFromModel(_aIModel);
		var args = new KernelArguments(settings)
		{
			["chapter_outline"] = outline,
			["description"] = storyDescription,
			["previous_chapter"] = previousChapter,
			["summary"] = summary
		};
		var kernel = kernelIn.Clone();
		//return await kernel.InvokePromptAsync<string>(ChapterWriterPrompt, args);
		var response = await kernel.InvokePromptAsync<string>(Prompts.ChapterWriterPrompt, args);
		return response;
	}
	
	[KernelFunction, Description("Summarize all the character details and plot events in the novel chapter")]
	public async Task<string> SummarizeChapter(Kernel kernel, [Description("Chapter text to summarize")] string chapterText)
	{
		var settings = new OpenAIPromptExecutionSettings { Store = true, Metadata = new Dictionary<string, string>() { ["Function"] = nameof(SummarizeChapter)}, MaxTokens = 1028};
		var args = new KernelArguments(settings)
		{
			["novel_chapter"] = chapterText
		};
		var response = await kernel.InvokePromptAsync<string>(SummaryPrompt, args);
		return response;
	}
	[KernelFunction, Description("Create an outline for a novel")]
	public async Task<string> CreateNovelOutline(Kernel kernel, [Description("The central topic or theme of the story")] string theme, [Description("A list of character details that must be included in the outline")] string characterDetails = "", [Description("Plot events that must occurr in the outline")] string plotEvents = "", [Description("The title of the novel")] string novelTitle = "", [Description("Number of chapters to include")] int chapters = 15, string additionalInstructions = "")
	{
		var promptFilter = new PromptFilter();
		var kernelClone = kernel.Clone();
        kernelClone.PromptRenderFilters.Add(promptFilter);
        var settings = GetPromptExecutionSettingsFromModel(_aIModel, 0.55);
		var args = new KernelArguments(settings)
		{
			["theme"] = theme,
			["characterDetails"] = characterDetails,
			["plotEvents"] = plotEvents,
			["novelTitle"] = novelTitle,
			["chapterCount"] = chapters
		};
        var instructions = string.IsNullOrEmpty(additionalInstructions) ? "" : $"## Additional User Instructions\n\n{additionalInstructions}\n";
        args["additionalInstructions"] = instructions;
        
        var response = await kernelClone.InvokePromptAsync<string>(Prompts.OutlineWriterPrompt, args);
		return response;
	}
	[KernelFunction, Description("Create an outline for a novel")]
	public async Task<string> ExpandChapterOutline(Kernel kernel, string outline, string storyDescription)
	{
		var settings = GetPromptExecutionSettingsFromModel(_aIModel);
		var args = new KernelArguments(settings)
		{
			["outline"] = outline,
			["storyDescription"] = storyDescription
		};
		var response = await kernel.InvokePromptAsync<string>(Prompts.ChapterOutlineExpansionPrompt, args);
		return response;
	}
	[KernelFunction, Description("Create title for a novel")]
	public async Task<string> GenerateTitle(Kernel kernel, string outline)
	{
		var settings = GetPromptExecutionSettingsFromModel(_aIModel, 0.7);
		
		var args = new KernelArguments(settings)
		{
			["outline"] = outline
		};
		var prompt = 
					"""
					Create a short title for a novel based on the outline below.
					Limit the title to 5 words or less.
					## Outline
					```
					{{$outline}} 
					```
					""";
		var response = await kernel.InvokePromptAsync<string>(prompt, args);
		return response;
	}
	private PromptExecutionSettings GetPromptExecutionSettingsFromModel(AIModel model, double tempurature = 1.0)
	{
		var providor = model.GetModelProvidors().FirstOrDefault();
		return providor switch
		{
			"GoogleAI" => new GeminiPromptExecutionSettings {  Temperature = tempurature },
			"MistralAI" => new MistralAIPromptExecutionSettings {  Temperature = tempurature },
			"OpenAI" or "AzureOpenAI" => new OpenAIPromptExecutionSettings { Store = true,  Temperature = tempurature},
			"AnthropicAI" => new AmazonClaudeExecutionSettings(){MaxTokensToSample = 8192, Temperature = (float)tempurature},
			_ => new OpenAIPromptExecutionSettings { Store = true,  Temperature = tempurature }
		};
	}
}
