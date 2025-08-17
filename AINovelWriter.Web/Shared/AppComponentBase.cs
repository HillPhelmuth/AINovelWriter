using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using static AINovelWriter.Shared.Models.EnumHelpers;
using static AINovelWriter.Shared.Models.FileHelper;
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
    [Inject]
    protected CosmosService CosmosService { get; set; } = default!;
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    [Inject]
    protected ContextMenuService ContextMenuService { get; set; } = default!;
    [Inject]
    protected NotificationService NotificationService { get; set; } = default!;

    protected bool ChangeFromSuggestedModel;
    protected bool IsBusy;
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }
    protected static AuthenticationState? AuthenticationState { get; set; }
    [Inject]
    private ImageGenService ImageGenService { get; set; } = default!;
    [Inject]
    protected ProtectedLocalStorage ProtectedLocalStorage { get; set; } = default!;
    protected async Task DownloadNovelToFile()
    {
        
        if (string.IsNullOrEmpty(AppState.NovelInfo.Text)) return;

        //var fileContent = await CreateAndCompressFilesAsync(AppState.NovelInfo.Text, AppState.NovelInfo.ImageUrl);
        using var client = new HttpClient();

        var imageBytes = await ImageGenService.GetImageBlob(AppState.NovelInfo.ImageUrl) /*await client.GetByteArrayAsync(AppState.NovelInfo.ImageUrl)*/;
        var pdfData = CreatePdf(imageBytes, AppState.NovelInfo.ChapterOutlines.Where(x => !string.IsNullOrEmpty(x.FullText)).Select(x => x.FullText).ToList()!, AppState.NovelInfo.Title);
        await JsRuntime.InvokeVoidAsync("downloadFile", $"{AppState.NovelInfo.Title}.pdf", pdfData);
    }
    protected async Task DownloadNovelToEpub()
    {
        if (string.IsNullOrEmpty(AppState.NovelInfo.Text)) return;
        var imageBytes = await ImageGenService.GetImageBlob(AppState.NovelInfo.ImageUrl);
        var epubData = CreateEpubFromNovel(AppState.NovelInfo, imageBytes);
        await JsRuntime.InvokeVoidAsync("downloadFile", $"{AppState.NovelInfo.Title}.epub", epubData);
    }
    protected static Dictionary<AIModel, string> AIModelDescriptions => GetEnumsWithDescriptions<AIModel>().ToDictionary(x => x.Key, y => y.Value);
    protected bool IsNovelComplete { get; set; }
    protected override async Task OnInitializedAsync()
    {
        AppState.PropertyChanged += HandlePropertyChanged;
        NovelWriterService.SendChapterText += HandleChapterFullText;
        NovelWriterService.SendOutline += HandleChapterOutline;
       
        await base.OnInitializedAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (AuthenticationStateTask is not null)
            {
                AuthenticationState ??= await AuthenticationStateTask;

                var userDataUserName = AuthenticationState?.User?.Identity?.Name;
                var picture = AuthenticationState?.User?.Claims
                    .Where(c => c.Type.Equals("picture"))
                    .Select(c => c.Value)
                    .FirstOrDefault() ?? string.Empty;
                var userProfile = await CosmosService.GetUserProfile(userDataUserName ?? string.Empty);
                if (userProfile is not null)
                {
                    AppState.UserData = userProfile;
                    AppState.UserData.ImagePath ??= picture;
                }
                else
                {
                    AppState.UserData.UserName = userDataUserName ?? string.Empty;
                    AppState.UserData.ImagePath ??= picture;
                }

               
            }
        }
        await base.OnAfterRenderAsync(firstRender);
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
        if (chapters == null || chapters.Count == 0) return;
        AppState.NovelInfo.ChapterOutlines.Clear();
        foreach (var chapter in chapters)
        {
            var title = chapter.Split("\n")[0];
            if (AppState.NovelInfo.ChapterOutlines.Any(x => x.Title == title)) continue;
            AppState.NovelInfo.ChapterOutlines.Add(new ChapterOutline(title, chapter, chapters.IndexOf(chapter)+1));
        }
        StateHasChanged();
    }
    private int _chapterIndex;
    protected void HandleChapterFullText(object? sender, ChapterEventArgs args)
    {
        if (_chapterIndex >= AppState.NovelInfo.ChapterOutlines.Count)
        {
            _chapterIndex = 0;
        }
        if (AppState.NovelInfo.ChapterOutlines.Count == 0) return;
        AppState.NovelInfo.ChapterOutlines[_chapterIndex].FullText = args.ChapterText;
        AppState.NovelInfo.ChapterOutlines[_chapterIndex].Summary = args.ChapterSummary;
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
    private static readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    protected static string AsHtml(string? text)
    {
        if (text == null) return "";
        var pipeline = _markdownPipeline;
        var result = Markdown.ToHtml(text, pipeline);
        return result;

    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            AppState.PropertyChanged -= HandlePropertyChanged;
            NovelWriterService.SendChapterText -= HandleChapterFullText;
            NovelWriterService.SendOutline -= HandleChapterOutline;
            NovelWriterService.TextToImageUrl -= HandleImageGen;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}