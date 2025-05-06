using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using AINovelWriter.Shared.Services;
using AINovelWriter.Shared.Models;
using Microsoft.JSInterop;
using Markdig;
using Radzen;

namespace AINovelWriter.Web.Pages;

public partial class BookPage
{
	private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
	private async Task Next()
	{
		var page = await JsRuntime.InvokeAsync<int>("next");
		Console.WriteLine($"Next Page: {page}");
		_pageInput = page-1;
	}

	private async Task Back()
	{
		var page = await JsRuntime.InvokeAsync<int>("back");
		Console.WriteLine($"Back Page: {page}");
		_pageInput = page-1;
	}

	private List<string> _pages = [];
	private bool _isSmall;
	private string _cardStyle = "height:36.375rem; position:relative; width:1100px; margin:auto; margin-top:0";
	protected override Task OnInitializedAsync()
	{
		if (string.IsNullOrEmpty(AppState.NovelInfo.ImageUrl)) NavigationManager.NavigateTo("");
		//if (AppState.NovelInfo.TextPages.Count <= 1)
		//{
		//	var is200K = AppState.WriterModel is AINovelWriter.Shared.Models.AIModel.Gpt4O or AINovelWriter.Shared.Models.AIModel.Gpt4Mini;
		//	AppState.NovelInfo.SplitIntoPagesByTokenLines(is4o: is200K);
		//}
		//_pages = AppState.NovelInfo.TextPages.Select(AsHtml).ToList();
		//if (AppState.NovelInfo.TextPages.FirstOrDefault()?.Contains("<img src=") == false)
		//	AppState.NovelInfo.TextPages.Insert(0, AppState.NovelInfo.ImgHtml);
		return base.OnInitializedAsync();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			if (AppState.NovelInfo.TextPages.Count <= 1)
			{
				var is200K = AppState.WriterModel is AIModel.Gpt41 or AIModel.Gpt4OMini or AIModel.Gpt41Mini or AIModel.Gpt41Nano or AIModel.Gpt4OChatGptLatest or AIModel.Gpt4OCurrent or AIModel.Grok3;
				AppState.NovelInfo.SplitIntoPagesByTokenLines(is4o: is200K);
			}
			_pages = AppState.NovelInfo.TextPages.Select(AsHtml).ToList();
			if (AppState.NovelInfo.TextPages.FirstOrDefault()?.Contains("<img src=") == false)
				AppState.NovelInfo.TextPages.Insert(0, AppState.NovelInfo.ImgHtml);
			StateHasChanged();
			await Task.Delay(1);
			await JsRuntime.InvokeVoidAsync("init");
			_isSmall = await JsRuntime.InvokeAsync<bool>("checkSize");
			if (_isSmall)
			{
				_cardStyle = "height:36.375rem; position:relative; width:40rem; margin:auto";
			}
		}
		await base.OnAfterRenderAsync(firstRender);
	}
	private string AsHtml(string? text)
	{
		if (text == null) return "";
		var pipeline = _markdownPipeline;
		var result = Markdown.ToHtml(text, pipeline);
		return result;

	}
	private int _pageInput;
	private async void HandleGoTo()
	{
		var page = _pageInput;
		await JsRuntime.InvokeVoidAsync("turnToPage", page);
	}

    private void ShowEvals()
    {
        DialogService.Open<NovelEvalPage>("Novel Evaluations",
            options: new DialogOptions() { Width = "85vw", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true});
    }

    private void ShowChatWithCharacter()
    {
        DialogService.Open<ChatWithNovelPage>("Chat with Character",
            options: new DialogOptions() { Width = "85vw", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true});
    }
	private async void HandleMediaChange(bool matches)
	{
		if (matches && !_isSmall)
		{
			_cardStyle = "height:36.375rem; position:relative; width:40rem; margin:auto";
			_isSmall = true;
			//await JsRuntime.InvokeVoidAsync("init");
		}
		else if (!matches && _isSmall)
		{
			_cardStyle = "height:36.375rem; position:relative; width:1100px; margin:auto";
			_isSmall = false;
			//await JsRuntime.InvokeVoidAsync("init");
		}

		StateHasChanged();
	}
}