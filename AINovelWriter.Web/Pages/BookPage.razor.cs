using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using AINovelWriter.Shared.Services;
using AINovelWriter.Shared.Models;
using Microsoft.JSInterop;
using Markdig;
using Radzen;
using static AINovelWriter.Shared.Models.FileHelper;

namespace AINovelWriter.Web.Pages;

public partial class BookPage
{
    private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    private double _zoom = 1.0;
    private async Task Next()
    {
        var page = await JsRuntime.InvokeAsync<int>("next");
        Console.WriteLine($"Next Page: {page}");
        _pageInput = page - 1;
    }

    private async Task Back()
    {
        var page = await JsRuntime.InvokeAsync<int>("back");
        Console.WriteLine($"Back Page: {page}");
        _pageInput = page - 1;
    }

    private List<string> _pages = [];
    private bool _isSmall;
    private string _cardStyle = "height:36.375rem; position:relative; width:1100px; margin:auto; margin-top:0";

    private string ViewPortClass => !_isSmall && _displayMode == PageDisplayMode.Single
        ? "flipbook-viewport-single"
        : "flipbook-viewport";
    private DotNetObjectReference<BookPage> DotNetRef => DotNetObjectReference.Create(this);
    private enum PageDisplayMode { Double, Single }
    private PageDisplayMode _displayMode = PageDisplayMode.Double;
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

    private bool _tempHide;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitBookViewer();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task InitBookViewer(string? mode = null)
    {
        _pages.Clear();
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
        if (mode is null)
            await JsRuntime.InvokeVoidAsync("init", DotNetRef);
        else switch (mode)
        {
            case "double":
            case "single":
                await SetDisplay(mode);
                break;
        }
        _isSmall = await JsRuntime.InvokeAsync<bool>("checkSize");
        if (_isSmall)
        {
            _cardStyle = "height:36.375rem; position:relative; width:40rem; margin:auto";
        }
    }

    private int _pageInput;
    private async void HandleGoTo()
    {
        try
        {
            var page = _pageInput;
            await JsRuntime.InvokeVoidAsync("turnToPage", page);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [JSInvokable]
    public void OnTurnEnd(int page)
    {
        _pageInput = page - 1;
        Console.WriteLine($"Page: {page}");
        StateHasChanged();
    }
    private void ShowEvals()
    {
        DialogService.Open<NovelEvalPage>("Novel Evaluations",
            options: new DialogOptions() { Width = "85vw", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });
    }

    private void ShowChatWithCharacter()
    {
        DialogService.Open<ChatWithNovelPage>("Chat with Character",
            options: new DialogOptions() { Width = "85vw", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });
    }

    private async Task Zoom(double zoom)
    {
        _zoom = zoom;
        await JsRuntime.InvokeVoidAsync("zoom", zoom);
    }
    private async void HandleMediaChange(bool matches)
    {
        try
        {
            switch (matches)
            {
                case true when !_isSmall:
                    _cardStyle = "height:36.375rem; position:relative; width:40rem; margin:auto";
                    _isSmall = true;
                    await SetDisplay("single");
                    break;
                case false when _isSmall:
                    _cardStyle = "height:36.375rem; position:relative; width:1100px; margin:auto";
                    _isSmall = false;
                    await SetDisplay("double");
                    break;
            }

            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SetDisplay(string display)
    {
        //_tempHide = true;
        //StateHasChanged();
        //await Task.Delay(100);
        //_tempHide = false;
        _displayMode = display == "double" ? PageDisplayMode.Double : PageDisplayMode.Single;
        await JsRuntime.InvokeVoidAsync("setDisplayMode", display);
        StateHasChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DotNetRef.Dispose();
        }
        base.Dispose(disposing);
    }
}