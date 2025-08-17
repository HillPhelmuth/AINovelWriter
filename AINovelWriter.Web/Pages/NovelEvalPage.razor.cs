using AINovelWriter.Evals;
using AINovelWriter.Shared.Models;
using HillPhelmuth.SemanticKernel.LlmAsJudgeEvals;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Radzen;
using Radzen.Blazor;

namespace AINovelWriter.Web.Pages;

public partial class NovelEvalPage
{
    private Dictionary<int, List<ResultScore>> _chapterResults = [];

    //private List<FlatChapterEval> FlatChapterEvals => _chapterResults.SelectMany(x =>
    //        x.Value.Select(y => new FlatChapterEval { ChapterNumber = x.Key, EvalName = y.EvalName, Score = y.AsEvalScore() })).ToList();
    private List<FlatChapterEval> _chapterEvals = [];
    private RadzenDataGrid<FlatChapterEval>? _grid;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private TooltipService TooltipService { get; set; } = default!;
    

    private void ShowToolTip(ElementReference elementReference, string text)
    {
        TooltipService.Open(elementReference, text);
    }
	protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (AppState.NovelInfo.NovelEval?.ChapterEvals.Count > 0)
            {
                _chapterEvals = AppState.NovelInfo.NovelEval.ChapterEvals;
                Console.WriteLine($"Chapter Evals already exist:\n{string.Join("\n", AppState.NovelInfo.NovelEval.ChapterEvals.Select(x => x.ToString()))}");
                await _grid.Reload();
            }
            else if (AppState.NovelInfo.ChapterEvals.Count > 0)
            {
                _chapterEvals = AppState.NovelInfo.ChapterEvals;
                await _grid.Reload();
            }
            else if (AppState.NovelInfo.ChapterOutlines.Count > 0)
            {
                await EvaluateNovel();
                
            }
            
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private string? GetChapterText(FlatChapterEval eval)
    {
        if (AppState.NovelInfo.ChapterOutlines.Count < eval.ChapterNumber)
            return "";
        return AppState.NovelInfo.ChapterOutlines[eval.ChapterNumber - 1].FullText;
    }

    private async Task ReEvaluateNovelChapter(int chapterNumber)
    {
        var chapter = AppState.NovelInfo.ChapterOutlines.FirstOrDefault(x => x.ChapterNumber == chapterNumber);
        if (chapter == null)
        {
            Console.WriteLine($"Chapter {chapterNumber} not found in ChapterOutlines.");
            return;
        }
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4.1-nano", Configuration["OpenAI:ApiKey"]!).Build();
        var service = new NovelEvalService(kernel);
        var details = AppState.NovelInfo.ConceptDescription;
        var text = chapter.FullText;
        var inputs = service.CreateInputModels(text, details);
        var results = await service.ExecuteEvals(inputs);
        var clarity = results.First(x => x.EvalName == "GptClarity").AsEvalScore();
        var creativity = results.First(x => x.EvalName == "GptCreativity").AsEvalScore();
        var engagement = results.First(x => x.EvalName == "GptEngagement").AsEvalScore();
        var relevance = results.First(x => x.EvalName == "GptRelevance").AsEvalScore();
        var writingDetail = results.First(x => x.EvalName == "GptWritingDetail").AsEvalScore();
        var characterDevelopment = results.First(x => x.EvalName == "GptCharacterDevelopment").AsEvalScore();
        var flatChapterEval = new FlatChapterEval
        {
            ChapterNumber = AppState.NovelInfo.ChapterOutlines.IndexOf(chapter) + 1,
            Clarity = clarity,
            Creativity = creativity,
            Engagement = engagement,
            Style = relevance,
            WritingDetail = writingDetail,
            CharacterDevelopment = characterDevelopment,
            ChapterText = text
        };
        // Replace the existing chapter eval.
        var existingEvalIndex = AppState.NovelInfo.NovelEval?.ChapterEvals.FindIndex(x => x.ChapterNumber == chapterNumber) ?? -1;
        if (existingEvalIndex >= 0)
        {
            AppState.NovelInfo.NovelEval.ChapterEvals[existingEvalIndex] = flatChapterEval;
        }
        else
        {
            AppState.NovelInfo.NovelEval?.ChapterEvals.Add(flatChapterEval);
        }
        StateHasChanged();
        await _grid.Reload();

    }
    private async Task EvaluateNovel()
    {
        _chapterEvals.Clear();
		IsBusy = true;
		StateHasChanged();
        await Task.Delay(1);
		var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4.1-nano", Configuration["OpenAI:ApiKey"]!).Build();
        var service = new NovelEvalService(kernel); 
        var details = AppState.NovelInfo.ConceptDescription;
        var novelInputs = service.CreateInputModels(AppState.NovelInfo.Text, details);
        var novelResults = await service.ExecuteEvals(novelInputs);
        AppState.NovelInfo.NovelEval = new FullNovelEval
		{
			CharacterDevelopment = novelResults.First(x => x.EvalName == "GptCharacterDevelopment").AsEvalScore(),
			Clarity = novelResults.First(x => x.EvalName == "GptClarity").AsEvalScore(),
			Creativity = novelResults.First(x => x.EvalName == "GptCreativity").AsEvalScore(),
			Engagement = novelResults.First(x => x.EvalName == "GptEngagement").AsEvalScore(),
			Style = novelResults.First(x => x.EvalName == "GptRelevance").AsEvalScore(),
			WritingDetail = novelResults.First(x => x.EvalName == "GptWritingDetail").AsEvalScore()
		};
		foreach (var chapter in AppState.NovelInfo.ChapterOutlines)
        {
            var text = chapter.FullText;
            
            var inputs = service.CreateInputModels(text, details);
            var results = await service.ExecuteEvals(inputs);
            _chapterResults[AppState.NovelInfo.ChapterOutlines.IndexOf(chapter) + 1] = results;
            var clarity = results.First(x => x.EvalName == "GptClarity").AsEvalScore();
            var creativity = results.First(x => x.EvalName == "GptCreativity").AsEvalScore();
            var engagement = results.First(x => x.EvalName == "GptEngagement").AsEvalScore();
            var relevance = results.First(x => x.EvalName == "GptRelevance").AsEvalScore();
            var writingDetail = results.First(x => x.EvalName == "GptWritingDetail").AsEvalScore();
            var characterDevelopment = results.First(x => x.EvalName == "GptCharacterDevelopment").AsEvalScore();
            var flatChapterEval = new FlatChapterEval
            {
                ChapterNumber = AppState.NovelInfo.ChapterOutlines.IndexOf(chapter) + 1,
                Clarity = clarity,
                Creativity = creativity,
                Engagement = engagement,
                Style = relevance,
                WritingDetail = writingDetail,
                CharacterDevelopment = characterDevelopment,
                ChapterText = text
            };
            AppState.NovelInfo.NovelEval.ChapterEvals.Add(flatChapterEval);
            StateHasChanged();
            await _grid.Reload();
        }
        //AppState.NovelInfo.ChapterEvals = _chapterEvals;
        //AppState.NovelInfo.NovelEval.ChapterEvals = _chapterEvals;
		IsBusy = false;
		StateHasChanged();
    }
}