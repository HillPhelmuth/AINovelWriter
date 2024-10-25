using Microsoft.AspNetCore.Components;
using Radzen;

namespace ChatComponents;

public partial class UploadImageWindow
{

    [Parameter]
    public FileUpload FileUpload { get; set; } = new();
    [Parameter]
    public EventCallback<FileUpload> FileUploadChanged { get; set; }
    [Parameter]
    public bool IsVisible { get; set; }
    //private FileUpload _fileUpload = new();
    private int maxFileSize = 1024 * 1024 * 500;
    private void Submit(FileUpload fileUpload)
    {
        Console.WriteLine(fileUpload.FileName + " Input Submitted");
        var base64String = fileUpload.FileBase64.Split(',')[1];
        var bytes = Convert.FromBase64String(base64String);
        fileUpload.FileBytes = bytes;
        FileUpload = fileUpload;
        FileUploadChanged.InvokeAsync(fileUpload);
        IsVisible = false;
        StateHasChanged();
    }
}