using AINovelWriter.Shared.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using AINovelWriter.Shared.Models.Enums;
using AINovelWriter.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.CognitiveServices.Speech.Diagnostics.Logging;

namespace AINovelWriter.Shared.Plugins;

public class EditorAgentPlugin(AppState appState, AIModel aIModel = AIModel.Gpt41)
{
    [KernelFunction, Description("Get the text of a chapter of the active novel")]
    public Task<string> GetChapterText(Kernel kernel, [Description("Chapter number")] int chapterNumber)
    {
        var outlines = appState.NovelInfo.ChapterOutlines;
        if (chapterNumber < 1 || chapterNumber > outlines.Count)
        {
            return Task.FromResult("Invalid chapter number. Please enter a valid chapter number.");
        }
        var chatper = outlines[chapterNumber - 1];
        return Task.FromResult(chatper.Text);
    }
    [KernelFunction, Description("Get the summary and review of the full active novel")]
    public async Task<string> GetFullNovelSummary(Kernel kernel,[Description("The type or context of the summary request")] ReviewContext summaryType = ReviewContext.FullCoverage)
    {
        var builder = Kernel.CreateBuilder();
        NovelWriterService.AddDefaultKernelServices(AIModel.Gpt41Mini, builder);
		var tempKernel = builder.Build();
        var novel = appState.NovelInfo;
        var promptTemplate =  Prompts.NovelContextSpecificReviewPrompt;
        var reviewNovelFunc = KernelFunctionFactory.CreateFromPrompt(promptTemplate);
        var contextPrompt = summaryType.GetPromptContext();

        var args = new KernelArguments { ["novelText"] = novel.Text, ["context"] = contextPrompt };
        var novelReview = await reviewNovelFunc.InvokeAsync<string>(tempKernel, args)!;
        var title = novel.Title;
        return $"# {title}\n\n{novelReview}";
    }
    [KernelFunction, Description("Get the evaluations of a novel broken down by chapter to help make editing decisions")]
    [return: Description("A markdown table of the evaluations for each chapter in the novel")]
    public Task<string> GetNovelEvals(Kernel kernel)
    {
        var novel = appState.NovelInfo;
        return Task.FromResult(novel.NovelEval?.ToString() ?? "No evaluations found.");
    }
    [KernelFunction, Description("Request a chapter review for feedback. Feedback will have three sections: Strengths, Weaknesses, and Suggestions for Improvement.")]
    public async Task<string> ReviewChapter(Kernel kernel, [Description("Chapter number")] int chapterNumber, [Description("additional notes that are important to the reviewer")] string additionalNotes = "")
    {
        var tempKernel = kernel.Clone();
        var outlines = appState.NovelInfo.ChapterOutlines;
        if (outlines.Count == 0)
        {
            return "No chapters found. Please create or select the novel.";
        }
        if (chapterNumber < 1 || chapterNumber > outlines.Count)
        {
            return "Invalid chapter number. Please enter a valid chapter number.";
        }
        var summary = outlines[chapterNumber - 1];
        var text = summary.FullText;
        var response = await tempKernel.InvokePromptAsync<string>(Prompts.ChapterImprovementPrompt, new KernelArguments(GetPromptExecutionSettingsKernel(tempKernel)) { ["chapterText"] = text, ["notes"] = additionalNotes });
        return response;
    }
    [KernelFunction, Description("Create a new chapter outline from feedback")]
    [return: Description("The new chapter outline from which you will re-write the chapter")]
    public async Task<string> CreateNewChapterOutlineFromFeedback(Kernel kernel, [Description("Chapter number")] int chapterNumber, [Description("Feedback from the reviewer")] string feedback)
    {
        var outlines = appState.NovelInfo.ChapterOutlines;
        if (outlines.Count == 0)
        {
            return "No chapters found. Please create or select the novel.";
        }
        if (chapterNumber < 1 || chapterNumber > outlines.Count)
        {
            return "Invalid chapter number. Please enter a valid chapter number.";
        }
        var summary = outlines[chapterNumber - 1];
        var text = summary.FullText;
        var response = await kernel.InvokePromptAsync<string>(Prompts.ChapterOutlineExpansionPrompt, new KernelArguments(GetPromptExecutionSettingsKernel(kernel)) { ["storyDescription"] = feedback, ["outline"] = summary.Text });
        return response;
    }
    [KernelFunction, Description("Replace a chapter with a re-written chapter. Be sure to always confirm with user before invoking.")]
    public string ReplaceChapter(Kernel kernel, [Description("Chapter number")] int chapterNumber, [Description("The new chapter text")] string newChapterText)
    {
        var outlines = appState.NovelInfo.ChapterOutlines;
        if (outlines.Count == 0)
        {
            return "no chapter outlines found";
        }
        if (chapterNumber < 1 || chapterNumber > outlines.Count)
        {
            return "Chapter number does not exist.";
        }
        var summary = outlines[chapterNumber - 1];
        summary.FullText = newChapterText;
        return "Chapter replaced successfully.";
    }
    [KernelFunction, Description("Suggest edits to a specific section of text of a chapter")]
    public string SuggestEditsToChapterText([Description("Chapter number")] int chapterNumber, [Description("The section of text to edit")] string sectionText, [Description("Additional notes for the editor")] string suggestions)
    {
        var outlines = appState.NovelInfo.ChapterOutlines;
        if (outlines.Count == 0)
        {
            return "No chapters found. Please create or select the novel.";
        }
        if (chapterNumber < 1 || chapterNumber > outlines.Count)
        {
            return "Invalid chapter number. Please enter a valid chapter number.";
        }

        var chapterOutline = appState.NovelInfo.ChapterOutlines.Find(ch => ch.ChapterNumber == chapterNumber);
        chapterOutline?.AddEditorNote(sectionText, suggestions);
        return $"Editor note added to chapter {chapterNumber}";
    }
    private PromptExecutionSettings GetPromptExecutionSettingsKernel(Kernel kernel)
    {
        var chatType = kernel.Services.GetRequiredService<IChatCompletionService>();
        return chatType switch
        {
            OpenAIChatCompletionService => new OpenAIPromptExecutionSettings { Store = true, ResponseFormat = "json_object" },
            GoogleAIGeminiChatCompletionService => new GeminiPromptExecutionSettings
            {
                ResponseMimeType = "application/json"
            },
            MistralAIChatCompletionService => new MistralAIPromptExecutionSettings
            {
               ResponseFormat = new { type = "json_object" }
            },
            _ => new OpenAIPromptExecutionSettings { Store = true, ResponseFormat = "json_object" }
        };
    }
}