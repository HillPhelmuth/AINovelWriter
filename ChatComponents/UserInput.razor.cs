using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;

namespace ChatComponents;

public partial class UserInput : ComponentBase
{
    [Parameter]
    public string HelperText { get; set; } = "";
    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public string ButtonLabel { get; set; } = "Submit";
    [Parameter]
    public EventCallback<string> MessageSubmit { get; set; }
    [Parameter]
    public EventCallback<string> MessageChanged { get; set; }
    [Parameter]
    public UserInputType UserInputType { get; set; }
    [Parameter]
    public UserInputFieldType UserInputFieldType { get; set; }
    [Parameter]
    public EventCallback<UserInputRequest> UserInputSubmit { get; set; }
    [Parameter]
    public EventCallback CancelRequest { get; set; }
    [Parameter]
    public bool IsRequired { get; set; } = true;

    private Popup _popup;
    private RadzenButton _button;
    private RadzenFormField _formField;
    private bool _isShow;
    protected override Task OnParametersSetAsync()
    {
        _requestForm.UserInputRequest.UserInputType = UserInputType;
        return base.OnParametersSetAsync();
    }

    private bool _isDisabled = false;

    private class RequestForm
    {
        public string? Content { get; set; }
        public bool ShowImageInput { get; set; }
        public UserInputRequest UserInputRequest { get; set; } = new();
            
    }

    private RequestForm _requestForm = new();
    private void Cancel()
    {
        CancelRequest.InvokeAsync();
    }
    private void ToggleInputType()
    {
        UserInputFieldType = UserInputFieldType == UserInputFieldType.TextBox
            ? UserInputFieldType.TextArea
            : UserInputFieldType.TextBox;
        StateHasChanged();
    }
    private void HandleFileAdded(FileUpload fileUpload)
    {
        _requestForm.UserInputRequest.FileUpload = fileUpload;
        _isShow = false;
        StateHasChanged();
    }
    //private async Task AddImage()
    //{
    //    var (fileName, bytes) = await FileService.OpenImageFileAsync();
    //    if (bytes.Length > 0)
    //    {
    //        _requestForm.UserInputRequest.FileUpload = new FileUpload
    //        {
    //            FileName = fileName,
    //            FileBytes = bytes,
    //            FileBase64 = Convert.ToBase64String(bytes)
    //        };
    //    }
    //}
    private void ShowImageInput()
    {

    }
    private void SubmitRequest(RequestForm form)
    {
        MessageSubmit.InvokeAsync(form.UserInputRequest.ChatInput);
        UserInputSubmit.InvokeAsync(form.UserInputRequest);
        _requestForm = new RequestForm
        {
            UserInputRequest =
            {
                UserInputType = UserInputType
            }
        };
        StateHasChanged();
    }
}

public enum UserInputType
{
    Chat,
    Ask,
    Both
}

public class UserInputRequest
{
    public string? ChatInput { get; set; }
    public FileUpload? FileUpload { get; set; }
    public UserInputType UserInputType { get; set;}
    public string? ImageUrlInput { get; set; }
}
public enum UserInputFieldType
{
    TextBox, TextArea
}