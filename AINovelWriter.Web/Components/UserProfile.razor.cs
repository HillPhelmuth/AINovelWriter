using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using static AINovelWriter.Shared.Models.FileHelper;
using static AINovelWriter.Shared.Services.NovelWriterService;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace AINovelWriter.Web.Components;

public partial class UserProfile
{
	[Inject]
	private CosmosService CosmosService { get; set; } = default!;
	[Inject]
	private DialogService Dialog { get; set; } = default!;
	[Inject]
	private ImageGenService ImageGenService { get; set; } = default!;
	[Inject]
	private NotificationService NotificationService { get; set; } = default!;

    protected override List<string> InterestingProperties => [nameof(AppState.UserData)];

    private bool _changeImage;
	private async Task SelectNovel(UserNovelData userNovelData)
	{
		var novel = await CosmosService.GetUserNovel(AppState.UserData.UserName, userNovelData.NovelId);
        if (novel.User is null)
        {
			NotificationService.Notify(NotificationSeverity.Info, "Novel Data updated. Try again.");
			Dialog.Close();
        }
		AppState.NovelInfo = novel;
        AppState.NovelOutline.Outline = string.Join("\n\n", AppState.NovelInfo.ChapterOutlines.Select(x => x.Text));
		AppState.NovelInfo.Outline = AppState.NovelOutline.Outline;
        Dialog.Close();
	}
	private async Task DeleteNovel(UserNovelData userNovelData)
	{
		var result = await Dialog.Confirm("Are you sure you want to delete this novel?", "Delete Novel");
		if (result == true)
		{
			var userData = AppState.UserData;
			await CosmosService.DeleteUserNovel(userData, userNovelData.NovelId);
			userData.SavedNovels.Remove(userNovelData);
			
		}
	}
	private async Task DownloadNovelToFile(UserNovelData userNovelData)
	{
		AppState.NovelInfo = await CosmosService.GetUserNovel(AppState.UserData.UserName!, userNovelData.NovelId);
		if (string.IsNullOrEmpty(AppState.NovelInfo.Text)) return;

        //var fileContent = await CreateAndCompressFilesAsync(AppState.NovelInfo.Text, AppState.NovelInfo.ImageUrl);
        using var client = new HttpClient();
 
        var imageBytes = await ImageGenService.GetImageBlob(AppState.NovelInfo.ImageUrl) /*await client.GetByteArrayAsync(AppState.NovelInfo.ImageUrl)*/;
		var pdfData = CreatePdf(imageBytes, AppState.NovelInfo.ChapterOutlines.Where(x => !string.IsNullOrEmpty(x.FullText)).Select(x => x.FullText).ToList()!, AppState.NovelInfo.Title);
        await JsRuntime.InvokeVoidAsync("downloadFile", $"{AppState.NovelInfo.Title}.pdf", pdfData);
	}
	private ImageUpdateForm _imageUpdateForm = new();
	private async void UpdateImage(ImageUpdateForm imageUpdateForm)
	{
		if (string.IsNullOrEmpty(imageUpdateForm.Url) && string.IsNullOrEmpty(imageUpdateForm.FileData.ImageBase64Data)) return;
		if (string.IsNullOrEmpty(imageUpdateForm.Url))
		{
			var fileDataImageBase64Data = imageUpdateForm.FileData.ImageBase64Data![(imageUpdateForm.FileData.ImageBase64Data.IndexOf("base64,", StringComparison.Ordinal) + 7)..];
			AppState.UserData.ImagePath = await ImageGenService.SaveUserImage(imageUpdateForm.FileData.FileName!, fileDataImageBase64Data);
			await CosmosService.SaveUser(AppState.UserData);
			StateHasChanged();
		}
		else
		{
			AppState.UserData.ImagePath = imageUpdateForm.Url;
		}
		_changeImage = false;
		StateHasChanged();
	}
	private class FileData
	{
		public string? FileName { get; set; }
		public string? ImageBase64Data { get; set; }
		public int FileSize { get; set; }
	}
	private class ImageUpdateForm
	{
		public string? Url { get; set; }
		public FileData FileData { get; set; } = new();
		public bool UseFileUpload { get; set; }
	}
}