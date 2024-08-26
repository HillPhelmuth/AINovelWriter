using AINovelWriter.Evals;
using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using PromptFlowEvalsAsPlugins;
using Radzen;
using Radzen.Blazor;

namespace AINovelWriter.Web.Pages;

public partial class NovelEvalPage
{
    private Dictionary<int, List<ResultScore>> _chapterResults = [];

    //private List<FlatChapterEval> FlatChapterEvals => _chapterResults.SelectMany(x =>
    //        x.Value.Select(y => new FlatChapterEval { ChapterNumber = x.Key, EvalName = y.EvalName, Score = y.ProbScore })).ToList();
    private List<FlatChapterEval> _chapterEvals = [];
    private RadzenDataGrid<FlatChapterEval>? _grid;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private TooltipService TooltipService { get; set; } = default!;
    private bool _isBusy;

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
            }
            else if (AppState.NovelInfo.ChapterEvals.Count > 0)
            {
                _chapterEvals = AppState.NovelInfo.ChapterEvals;
			}
            else if (AppState.NovelInfo.ChapterOutlines.Count > 0)
            {
                await EvaluateNovel();
                
            }
            
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task EvaluateNovel()
    {
        _chapterEvals.Clear();
		_isBusy = true;
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
			Relevance = novelResults.First(x => x.EvalName == "GptRelevance").ProbScore,
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
                Relevance = relevance,
                WritingDetail = writingDetail,
                CharacterDevelopment = characterDevelopment,
                ChapterText = text
            };
            _chapterEvals.Add(flatChapterEval);
            StateHasChanged();
            await _grid.Reload();
        }
        //AppState.NovelInfo.ChapterEvals = _chapterEvals;
        AppState.NovelInfo.NovelEval.ChapterEvals = _chapterEvals;
		_isBusy = false;
		StateHasChanged();
    }
}