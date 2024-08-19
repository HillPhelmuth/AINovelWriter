using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using static AINovelWriter.Shared.Models.EnumHelpers;

namespace AINovelWriter.Web.Pages;

public partial class NovelConceptPage
{
	private class NovelIdea
	{
		public NovelGenre NovelGenre { get; set; }
		public string? Subgenre { get; set; }
	}

	private class NovelIdeaForm
	{
		public GenreCategoryItem? NovelCategory { get; set; }
		public List<Genre> SubGenres { get; set; } = [];
	}
	private NovelIdeaForm _novelIdeaForm = new();
	private bool _isBusy;
	private bool _showOutline;
	private NovelIdea _novelIdea = new();
	private List<NovelGenre> _genres = Enum.GetValues<NovelGenre>().ToList();
	private Dictionary<NovelGenre, List<string>> _genreWithSubgenres = Enum.GetValues<NovelGenre>().ToDictionary(x => x, y => y.GetSubGenreList());
	RadzenButton _button;
	Popup _popup;
	private RadzenDropDown<AIModel> _aiModelField;
	private static Dictionary<AIModel, string> AIModelDescriptions => GetEnumsWithDescriptions<AIModel>().ToDictionary(x => x.Key, y => y.Value);

	public Dictionary<string, string> AuthorStyles = new()
		{
			{ nameof(Prompts.Authors.JoeAbercrombie), Prompts.Authors.JoeAbercrombie },
			{ nameof(Prompts.Authors.LeeChild), Prompts.Authors.LeeChild },
			{ nameof(Prompts.Authors.StevenErikson), Prompts.Authors.StevenErikson },
			{ nameof(Prompts.Authors.BrandonSanderson), Prompts.Authors.BrandonSanderson }
		};
	private CancellationTokenSource _cancellationTokenSource = new();
	private async Task SubmitIdea(NovelIdeaForm novelIdea)
	{
		_isBusy = true;
		StateHasChanged();

		await _popup.CloseAsync();
		await TryGenerateConcepts(novelIdea);
		_isBusy = false;
		StateHasChanged();
	}
	private int _attempts = 0;
	private async Task TryGenerateConcepts(NovelIdeaForm novelIdea)
	{
		try
		{
			AppState.NovelConcepts = await NovelWriterService.GenerateNovelIdea(novelIdea.NovelCategory!, novelIdea.SubGenres);
			AppState.NovelConcepts.Genre = novelIdea.NovelCategory!.Category;
			StateHasChanged();

		}
		catch (Exception e)
		{
			_attempts++;
			if (_attempts < 3)
			{
				await TryGenerateConcepts(novelIdea);
				return;
			}
		}
		_attempts = 0;
	}

	private async void CreateOutline(NovelConcepts novelConcepts)
	{
		_isBusy = true;
		StateHasChanged();
		await Task.Delay(1);
		AppState.NovelInfo = new NovelInfo() { User = AppState.UserData.UserName };
		var theme = $"{novelConcepts.Genre}\n{novelConcepts.SubGenre} {novelConcepts.Theme}";
		AppState.NovelOutline.Outline = await NovelWriterService.CreateNovelOutline(theme, novelConcepts.Characters, novelConcepts.PlotEvents, novelConcepts.Title, novelConcepts.ChapterCount, novelConcepts.OutlineAIModel);
		AppState.NovelInfo.Outline = AppState.NovelOutline.Outline;
		AppState.NovelInfo.Title = novelConcepts.Title;
		AppState.NovelInfo.ConceptDescription = novelConcepts.ToString();
		AppState.NovelInfo.AuthorStyle = novelConcepts.AuthorStyle;
        _isBusy = false;
		_showOutline = true;
		await _aiModelField.Element.FocusAsync();
		StateHasChanged();
	}
	private async void GenerateNovel(NovelOutline novelWriter)
	{
		_isBusy = true;

		StateHasChanged();
		var ctoken = _cancellationTokenSource.Token;
		AppState.NovelOutline = novelWriter;
		AppState.WriterModel = novelWriter.WriterAIModel;
		AppState.NovelInfo.Text = "";
		NavigationManager.NavigateTo("stream");
		await Task.Delay(1);
		//await foreach (var token in NovelWriterService.WriteNovel(AppState.NovelOutline.Outline, AppState.NovelOutline.WriterAIModel, ctoken))
		//{
		//    AppState.NovelInfo.Text += token;
		//    await InvokeAsync(StateHasChanged);
		//}
		_isBusy = false;
		StateHasChanged();
	}
}
