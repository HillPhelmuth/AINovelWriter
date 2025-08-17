using System.Text;
using System.Text.Json.Serialization;
using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AINovelWriter.Web.Pages;

public partial class Home
{
    private bool _isReverse;


    private FileUploadForm _fileUploadForm = new();
	private class FileUploadForm
    {
		public string? FileName { get; set; }
		public string? FileContent { get; set; }
		public long? FileSize { get; set; }
	}

	private async void UploadEpub(FileUploadForm fileUploadForm)
	{
		IsBusy = true;
		StateHasChanged();
		await Task.Delay(1);
		var title = Path.GetFileNameWithoutExtension(fileUploadForm.FileName);
		AppState.NovelInfo = await NovelWriterService.ReverseEngineerNovel(ExtractBase64String(fileUploadForm.FileContent), title);
        AppState.NovelInfo.IsComplete = true;
        AppState.NovelOutline.Outline = AppState.NovelInfo.Outline;
        IsBusy = false;
       var isNavigate = await DialogService.Confirm("Sometimes and epub file will be extracted with unnecessary chapters and sections. It's often best to modify or remove them.\n\n Do you want to edit to the uploaded novel?", "Edit Novel Upload");
        if (isNavigate == true)
        {
            NavigationManager.NavigateTo("/edit/3");
        }
		StateHasChanged();
	}
	// Extract base64 string from data url
	private string ExtractBase64String(string dataUrl)
	{
		var base64 = dataUrl.Split(",")[1];
		return base64;
	}


}