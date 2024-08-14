using AINovelWriter.Evals;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using PromptFlowEvalsAsPlugins;
using Radzen.Blazor;

namespace AINovelWriter.Web.Pages;

public partial class NovelEvalPage
{
	private Dictionary<int, List<ResultScore>> _chapterResults = [];

    //private List<FlatChapterEval> FlatChapterEvals => _chapterResults.SelectMany(x =>
    //        x.Value.Select(y => new FlatChapterEval { ChapterNumber = x.Key, EvalName = y.EvalName, Score = y.ProbScore })).ToList();
    private List<FlatChapterEval> _chapterEvals = [];
	private RadzenDataGrid<FlatChapterEval>? _grid;
    private class FlatChapterEval
    {
        public int ChapterNumber { get; set; }
        public double CharacterDevelopment { get; set; }
		public double Clarity { get; set; }
        public double Creativity { get; set; }
        public double Engagement { get; set; }
        public double Relevance { get; set; }
        public double WritingDetail { get; set; }
		public double Overall => (CharacterDevelopment + Clarity + Creativity + Engagement + Relevance + WritingDetail) / 6;
		public string? ChapterText { get; set; }
    }
	[Inject]
	private IConfiguration Configuration { get; set; } = default!;
	 protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			var kernel = Kernel.CreateBuilder()
				.AddOpenAIChatCompletion("gpt-3.5-turbo", Configuration["OpenAI:ApiKey"]).Build();
			var service = new NovelEvalService(kernel);
			foreach (var chapter in AppState.NovelInfo.ChapterOutlines)
			{
				var text = chapter.FullText;
				var details = AppState.NovelInfo.ConceptDescription;
				var inputs = service.CreateInputModels(text, details);
				var results = await service.ExecuteEvals(inputs);
				_chapterResults[AppState.NovelInfo.ChapterOutlines.IndexOf(chapter)+1] = results;
				var clarity = results.First(x => x.EvalName == "GptClarity").ProbScore;
				var creativity = results.First(x => x.EvalName == "GptCreativity").ProbScore;
				var engagement = results.First(x => x.EvalName == "GptEngagement").ProbScore;
				var relevance = results.First(x => x.EvalName == "GptRelevance").ProbScore;
				var writingDetail = results.First(x => x.EvalName == "GptWritingDetail").ProbScore;
				var characterDevelopment = results.First(x => x.EvalName == "GptCharacterDevelopment").ProbScore;
				var flatChapterEval = new FlatChapterEval
                {
                    ChapterNumber = AppState.NovelInfo.ChapterOutlines.IndexOf(chapter)+1,
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
				if (_grid != null && flatChapterEval.ChapterNumber % 2 == 0)
				{
					await _grid.Reload();
				}
			}
		}
		await base.OnAfterRenderAsync(firstRender);
	}
}