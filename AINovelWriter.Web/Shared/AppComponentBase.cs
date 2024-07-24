using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Text.Json;

namespace AINovelWriter.Web.Shared;

public abstract class AppComponentBase : ComponentBase, IDisposable
{
	[Inject]
	protected AppState AppState { get; set; } = default!;
	[Inject]
	protected INovelWriter NovelWriterService { get; set; } = default!;
	[Inject]
	protected IJSRuntime JsRuntime { get; set; } = default!;
	[Inject]
	protected NavigationManager NavigationManager { get; set; } = default!;
	protected bool IsNovelComplete { get; set; }
	protected override Task OnInitializedAsync()
	{
		AppState.PropertyChanged += HandlePropertyChanged;
		NovelWriterService.SendChapterText += HandleUpdate;
		NovelWriterService.SendChapters += HandleChapterOutline;
		return base.OnInitializedAsync();
	}
	protected virtual List<string> InterestingProperties => [];
	protected virtual void HandlePropertyChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (!InterestingProperties.Contains(args.PropertyName!)) return;
		InvokeAsync(StateHasChanged);
	}
	protected void HandleChapterOutline(string text)
	{
		var chapters = JsonSerializer.Deserialize<List<string>>(text);
		foreach (var chapter in chapters)
		{
			var title = chapter.Split("\n")[0];
			AppState.NovelInfo.ChapterOutlines.Add(new OutlineChapter(title, chapter));
		}
		StateHasChanged();
	}
	private int _chapterIndex;
	protected void HandleUpdate(object? sender, string args)
	{
		AppState.NovelInfo.ChapterOutlines[_chapterIndex].FullText = args;
		_chapterIndex++;
		StateHasChanged();
	}
	private void HandleImageGen(object? sender, string url)
	{
		AppState.NovelInfo.ImageUrl = url;
		AppState.NovelInfo.SplitIntoPagesByLines(21);
		AppState.NovelInfo.IsComplete = true;
		StateHasChanged();
		//AppState.NovelInfo.TextPages = StringHelpers.SplitStringIntoPagesByLines(AppState.NovelInfo.Text, 20);

	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			AppState.PropertyChanged -= HandlePropertyChanged;
			NovelWriterService.SendChapterText -= HandleUpdate;
			NovelWriterService.SendChapters -= HandleChapterOutline;
			NovelWriterService.TextToImageUrl -= HandleImageGen;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}