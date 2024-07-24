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
	}

	private bool _isBusy;
	private bool _showOutline;
	private NovelIdea _novelIdea = new();
	private List<NovelGenre> _genres = Enum.GetValues<NovelGenre>().ToList();
	RadzenButton _button;
	Popup _popup;
	private RadzenDropDown<AIModel> _aiModelField;
	private static Dictionary<AIModel, string> AIModelDescriptions => GetEnumsWithDescriptions<AIModel>().ToDictionary(x => x.Key, y => y.Value);
	private CancellationTokenSource _cancellationTokenSource = new();
	private async Task SubmitIdea(NovelIdea novelIdea)
	{
		_isBusy = true;
		StateHasChanged();

		await _popup.CloseAsync();
		await TryGenerateConcepts(novelIdea);
		_isBusy = false;
		StateHasChanged();
	}
	private int _attempts = 0;
	private async Task TryGenerateConcepts(NovelIdea novelIdea)
	{
		try
		{
			AppState.NovelConcepts = await NovelWriterService.GenerateNovelIdea(novelIdea.NovelGenre);
			AppState.NovelConcepts.Genre = novelIdea.NovelGenre;
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
		AppState.NovelOutline.Outline = await NovelWriterService.CreateNovelOutline(novelConcepts.Theme, novelConcepts.Characters, novelConcepts.PlotEvents, novelConcepts.Title, novelConcepts.ChapterCount, novelConcepts.OutlineAIModel);
		AppState.NovelInfo.Outline = AppState.NovelOutline.Outline;
		AppState.NovelInfo.Title = novelConcepts.Title;
		AppState.NovelInfo.ConceptDescription = novelConcepts.ToString();
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
		NavigationManager.NavigateTo("stream");
		await Task.Delay(1);
		//await foreach (var token in NovelWriterService.WriteNovel(AppState.NovelOutline.Outline, AppState.NovelOutline.WriterAIModel, ctoken))
		//{
		//	AppState.NovelInfo.Text += token;
		//	await InvokeAsync(StateHasChanged);
		//}
		_isBusy = false;
		StateHasChanged();
	}
}