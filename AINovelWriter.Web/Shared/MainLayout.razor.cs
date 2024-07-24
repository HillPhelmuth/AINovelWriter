using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.ComponentModel;
using AINovelWriter.Shared.Models;
using AINovelWriter.Web.Components;

namespace AINovelWriter.Web.Shared;

public partial class MainLayout
{
	[Inject]
	private AppState AppState { get; set; } = default!;
	[Inject]
	private DialogService DialogService { get; set; } = default!;
	[Inject]
	private CosmosService CosmosService { get; set; } = default!;
	[Inject]
	private NotificationService NotificationService { get; set; } = default!;
	private bool _hasOutline;
	private bool _hasNovel;
	private bool _hasCover;
	private string _currentPage;
	private string _bodyStyle = "padding:.75rem .5rem;";
	private int _currentMaxWidth = int.MaxValue;
	private int _currrentMaxHeight = int.MaxValue;
	protected override void OnInitialized()
	{
		AppState.PropertyChanged += AppState_PropertyChanged;
		base.OnInitialized();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{

		}
		await base.OnAfterRenderAsync(firstRender);
	}
	private async void Reset()
	{
		var confirm = await DialogService.Confirm("Resetting will cause you your work!", "Are you sure you want to reset the novel?");
		if (confirm == true)
		{
			AppState.NovelInfo = new NovelInfo() { User = AppState.UserData.UserName };
			AppState.NovelConcepts = new NovelConcepts();
			AppState.NovelOutline = new NovelOutline();
			StateHasChanged();
		}
	}
	private void AppState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(AppState.NovelOutline):
				_hasOutline = !string.IsNullOrEmpty(AppState.NovelOutline.Outline);
				StateHasChanged();
				break;
			case nameof(AppState.CurrentPage):
				_currentPage = AppState.CurrentPage!;
				_bodyStyle = _currentPage == "book" ? "padding:0;overflow:hidden" : "padding:.75rem .5rem;";
				StateHasChanged();
				break;
		}
	}
	private void ShowProfile()
	{
		DialogService.Open<UserProfile>("", options: new DialogOptions { CloseDialogOnOverlayClick = true, Draggable = true, Resizable = true });
	}
	private async Task SaveNovel()
	{
		var result = await CosmosService.SaveUserNovel(AppState.NovelInfo, AppState.UserData);
		if (result.Item1)
		{
			NotificationService.Notify(NotificationSeverity.Success, "Novel Saved", $"Novel {AppState.NovelInfo.Title} Saved successfully!");
		}
		else
		{
			NotificationService.Notify(NotificationSeverity.Error, $"Error Saving Novel", result.Item2);
		}
	}
	private enum PageMedia { Height, Width }
	private void HandleMediaChange(PageMedia pageMedia, int size, bool isMatch)
	{
		Console.WriteLine($"HandleMediaChange triggers: {pageMedia} < {size} = {isMatch}");
		switch (pageMedia)
		{
			case PageMedia.Width:
				{
					if (isMatch && _currentMaxWidth >= size)
						_currentMaxWidth = size;
					break;
				}
			case PageMedia.Height:
				{
					if (isMatch && _currrentMaxHeight >= size)
						_currrentMaxHeight = size;
					break;
				}
		}
	}
}