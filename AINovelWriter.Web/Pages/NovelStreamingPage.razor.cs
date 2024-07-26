using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text.Json;
using System.Threading;
using static AINovelWriter.Shared.Models.FileHelper;
using static AINovelWriter.Shared.Services.NovelWriterService;


namespace AINovelWriter.Web.Pages;

public partial class NovelStreamingPage
{
	private async Task DownloadNovelToFile()
	{
		if (string.IsNullOrEmpty(AppState.NovelInfo.Text)) return;
		
		//var novelJson = JsonSerializer.Serialize(AppState.NovelInfo, new JsonSerializerOptions { WriteIndented = true });
		//var fileContent = FileHelper.GenerateTextFile(novelJson);
		var fileContent = await CreateAndCompressFilesAsync(AppState.NovelInfo.Text, AppState.NovelInfo.ImageUrl);
		//var fileContent = await pdf.CreatePdfDocument(AppState.NovelInfo);
		await JsRuntime.InvokeVoidAsync("downloadFile", $"{AppState.NovelInfo.Title}.zip", fileContent);
	}
	
	private CancellationTokenSource _cancellationTokenSource = new();
	private bool _isCheat;
	private List<string> _pages = [];
	private bool _isBusy;
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && string.IsNullOrEmpty(AppState.NovelInfo.Text))
		{
			await GenerateNovelAsync();
		}
		await base.OnAfterRenderAsync(firstRender);
	}

	private async Task GenerateNovelAsync()
	{
		if (string.IsNullOrEmpty(AppState.NovelOutline.Outline)) return;
		AppState.NovelInfo.Text = "";
		_isBusy = true;
		StateHasChanged();
		var ctoken = _cancellationTokenSource.Token;
		await foreach (var token in NovelWriterService.WriteNovel(AppState.NovelOutline.Outline!, AppState.NovelOutline.WriterAIModel, ctoken))
		{
			AppState.NovelInfo.Text += token;
			await InvokeAsync(StateHasChanged);
		}
		AppState.NovelInfo.IsComplete = true;
		_isBusy = false;
		StateHasChanged();

	}

	private async void Cheat()
	{

		_isCheat = true;
		var tempJson = await File.ReadAllTextAsync("CheatNovel2.json");
		AppState.NovelInfo = JsonSerializer.Deserialize<NovelInfo>(tempJson)!;
		AppState.NovelInfo.IsComplete = true;
		var listString = JsonSerializer.Serialize(SplitMarkdownByHeaders(AppState.NovelInfo.Outline));
		HandleChapterOutline(listString);
		StateHasChanged();
		await Task.Delay(1);
		//foreach (var chapter in chapters)
		//{
		//	var title = chapter.Split("\n")[0];
		//	_chapterOutlines.Add(new OutlineChapter(title, chapter) { FullText = chapter });
		//}
		//_chapterOutlines = _chapterOutlines.Select(x => new OutlineChapter(x.Title, x.Text) { FullText = TempStory }).ToList();
		_pages = StringHelpers.SplitStringIntoPagesByWords(AppState.NovelInfo.Text, 175);
		_pages.Insert(0, AppState.NovelInfo.ImgHtml);
		StateHasChanged();
		await Task.Delay(1);
		//await JsRuntime.InvokeVoidAsync("init");
	}
	private void Cancel()
	{
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
	}
	private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
	private string AsHtml(string? text)
	{
		if (text == null) return "";
		var pipeline = _markdownPipeline;
		var result = Markdown.ToHtml(text, pipeline);
		return result;

	}
}