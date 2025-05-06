using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace AINovelWriter.Web.Components;
public partial class FindSharedNovels
{
    private List<SharedNovelInfo> _sharedNovels = [];
    private RadzenDataGrid<SharedNovelInfo>? _grid;
    private string _busyText = "Loading shared novels...";
    [Inject] 
    private ImageGenService ImageGenService { get; set; } = default!;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsBusy = true;
            _sharedNovels = await CosmosService.GetAllSharedNovels();
            IsBusy = false;
            await _grid!.Reload();
            StateHasChanged();
            // This is where you can put your JavaScript interop call
            // For example, to call a JavaScript function named 'initialize'
            // await JSRuntime.InvokeVoidAsync("initialize");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    private Task SelectNovel(SharedNovelInfo sharedNovelInfo)
    {
        var novel = sharedNovelInfo.ToNovelInfo(AppState.UserData.UserName!);
        
        if (novel.User is null)
        {
            NotificationService.Notify(NotificationSeverity.Info, "Novel Data updated. Try again.");
            DialogService.Close();
        }
        AppState.NovelInfo = novel;
        
        DialogService.Close();
        return Task.CompletedTask;
    }
    private async Task DownloadNovelToFile(SharedNovelInfo sharedNovelInfo)
    {
        //AppState.NovelInfo = await CosmosService.GetUserNovel(AppState.UserData.UserName!, userNovelData.NovelId);
        if (string.IsNullOrEmpty(sharedNovelInfo.Text)) return;

        //var fileContent = await CreateAndCompressFilesAsync(AppState.NovelInfo.Text, AppState.NovelInfo.ImageUrl);
        using var client = new HttpClient();

        var imageBytes = await ImageGenService.GetImageBlob(sharedNovelInfo.ImageUrl) /*await client.GetByteArrayAsync(AppState.NovelInfo.ImageUrl)*/;
        var splitMarkdownByHeaders = sharedNovelInfo.Text.Split("##").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var chapterCount = splitMarkdownByHeaders.Count;
        Console.WriteLine($"Chapter Count: {chapterCount}");
        var pdfData = FileHelper.CreatePdf(imageBytes, splitMarkdownByHeaders, sharedNovelInfo.Title);
        await JsRuntime.InvokeVoidAsync("downloadFile", $"{sharedNovelInfo.Title}.pdf", pdfData);
    }
    private async Task DownloadNovelToEpub(SharedNovelInfo sharedNovelInfo)
    {
        var novel = sharedNovelInfo.ToNovelInfo(AppState.UserData.UserName!);
        //AppState.NovelInfo = await CosmosService.GetUserNovel(AppState.UserData.UserName!, userNovelData.NovelId);
        if (string.IsNullOrEmpty(sharedNovelInfo.Text)) return;
        var imageBytes = await ImageGenService.GetImageBlob(sharedNovelInfo.ImageUrl);
        var epubData = FileHelper.CreateEpubFromNovel(novel, imageBytes);
        await JsRuntime.InvokeVoidAsync("downloadFile", $"{sharedNovelInfo.Title}.epub", epubData);
    }
}
