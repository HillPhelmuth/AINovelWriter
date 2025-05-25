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
    private Dictionary<int, List<ResultScore<CustomEvalOutput>>> _chapterResults = [];

    //private List<FlatChapterEval> FlatChapterEvals => _chapterResults.SelectMany(x =>
    //        x.Value.Select(y => new FlatChapterEval { ChapterNumber = x.Key, EvalName = y.EvalName, Score = y.ProbScore })).ToList();
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
                await EvaluateNovelWithCriticalEvals();
                
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

    private async Task EvaluateNovelWithCriticalEvals()
    {
        _chapterEvals.Clear();
        IsBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", Configuration["OpenAI:ApiKey"]!).Build();
        var service = new NovelEvalService(kernel);
        var details = AppState.NovelInfo.ConceptDescription;
        var novelInputs = service.CreateInputModels(AppState.NovelInfo.Text, details);
        var novelResults = await service.ExecuteEvals(novelInputs);
        AppState.NovelInfo.NovelEval = new FullNovelEval
        {
            CharacterDevelopment = novelResults.First(x => x.EvalName.StartsWith("GptCharacterDevelopment")).Score,
            Clarity = novelResults.First(x => x.EvalName.StartsWith("GptClarity")).Score,
            Creativity = novelResults.First(x => x.EvalName.StartsWith("GptCreativity")).Score,
            Engagement = novelResults.First(x => x.EvalName.StartsWith("GptEngagement")).Score,
            Style = novelResults.First(x => x.EvalName.StartsWith("GptRelevance")).Score,
            WritingDetail = novelResults.First(x => x.EvalName.StartsWith("GptWritingDetail")).Score,

        };
        foreach (var chapter in AppState.NovelInfo.ChapterOutlines)
        {
            var text = chapter.FullText;

            var inputs = service.CreateInputModels(text, details);
            var results = await service.ExecuteEvals(inputs);
            _chapterResults[AppState.NovelInfo.ChapterOutlines.IndexOf(chapter) + 1] = results;
            var clarity = results.First(x => x.EvalName.StartsWith("GptClarity"));
            var creativity = results.First(x => x.EvalName.StartsWith("GptCreativity"));
            var engagement = results.First(x => x.EvalName.StartsWith("GptEngagement"));
            var relevance = results.First(x => x.EvalName.StartsWith("GptRelevance"));
            var writingDetail = results.First(x => x.EvalName.StartsWith("GptWritingDetail"));
            var characterDevelopment = results.First(x => x.EvalName.StartsWith("GptCharacterDevelopment"));
            var flatChapterEval = new FlatChapterEval
            {
                ChapterNumber = AppState.NovelInfo.ChapterOutlines.IndexOf(chapter) + 1,
                Clarity = clarity.Score,
                ClarityMajorWeaknesses = clarity.Result.MajorWeaknesses,
                ClarityMinorWeaknesses = clarity.Result.MinorWeaknesses,
                Creativity = creativity.Score,
                CreativityMajorWeaknesses = creativity.Result.MajorWeaknesses,
                CreativityMinorWeaknesses = creativity.Result.MinorWeaknesses,
                Engagement = engagement.Score,
                EngagementMajorWeaknesses = engagement.Result.MajorWeaknesses,
                EngagementMinorWeaknesses = engagement.Result.MinorWeaknesses,
                Style = relevance.Score,
                StyleMajorWeaknesses = relevance.Result.MajorWeaknesses,
                StyleMinorWeaknesses = relevance.Result.MinorWeaknesses,
                WritingDetail = writingDetail.Score,
                WritingDetailMajorWeaknesses = writingDetail.Result.MajorWeaknesses,
                WritingDetailMinorWeaknesses = writingDetail.Result.MinorWeaknesses,
                CharacterDevelopment = characterDevelopment.Score,
                CharacterDevelopmentMajorWeaknesses = characterDevelopment.Result.MajorWeaknesses,
                CharacterDevelopmentMinorWeaknesses = characterDevelopment.Result.MinorWeaknesses,
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
    private async Task EvaluateNovel()
    {
        _chapterEvals.Clear();
		IsBusy = true;
		StateHasChanged();
        await Task.Delay(1);
		var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", Configuration["OpenAI:ApiKey"]!).Build();
        var service = new NovelEvalService(kernel); 
        var details = AppState.NovelInfo.ConceptDescription;
        var novelInputs = service.CreateInputModels(AppState.NovelInfo.Text, details);
        var novelResults = await service.ExecuteEvals(novelInputs);
        AppState.NovelInfo.NovelEval = new FullNovelEval
		{
			CharacterDevelopment = novelResults.First(x => x.EvalName == "GptCharacterDevelopment").ProbScore,
			Clarity = novelResults.First(x => x.EvalName == "GptClarity").ProbScore,
			Creativity = novelResults.First(x => x.EvalName == "GptCreativity").ProbScore,
			Engagement = novelResults.First(x => x.EvalName == "GptEngagement").ProbScore,
			Style = novelResults.First(x => x.EvalName == "GptRelevance").ProbScore,
			WritingDetail = novelResults.First(x => x.EvalName == "GptWritingDetail").ProbScore,
			
		};
		foreach (var chapter in AppState.NovelInfo.ChapterOutlines)
        {
            var text = chapter.FullText;
            
            var inputs = service.CreateInputModels(text, details);
            var results = await service.ExecuteEvals(inputs);
            _chapterResults[AppState.NovelInfo.ChapterOutlines.IndexOf(chapter) + 1] = results;
            var clarity = results.First(x => x.EvalName == "GptClarity").ProbScore;
            var creativity = results.First(x => x.EvalName == "GptCreativity").ProbScore;
            var engagement = results.First(x => x.EvalName == "GptEngagement").ProbScore;
            var relevance = results.First(x => x.EvalName == "GptRelevance").ProbScore;
            var writingDetail = results.First(x => x.EvalName == "GptWritingDetail").ProbScore;
            var characterDevelopment = results.First(x => x.EvalName == "GptCharacterDevelopment").ProbScore;
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