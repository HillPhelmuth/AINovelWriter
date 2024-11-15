using ChatComponents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AINovelWriter.Web.Components;
public partial class EditorAgent
{
    private ChatView _chatView;
    private bool _isBusy;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage("First, get the full summary of the active novel. Then, introduce yourself, your purpose, and the services you provide");
            var response = NovelWriterService.ExecuteEditorAgentChat(chatHistory);
            await HandleStreaming(response);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async void HandleChatInput(UserInputRequest userInputRequest)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        if (!string.IsNullOrWhiteSpace(userInputRequest.ImageUrlInput) || !string.IsNullOrWhiteSpace(userInputRequest.FileUpload?.FileBase64))
        {
            Console.WriteLine("Image Input");
            var imageUrl = userInputRequest.ImageUrlInput ?? userInputRequest.FileUpload?.FileBase64;
            _chatView.ChatState.AddUserMessage(new TextContent(userInputRequest.ChatInput), new ImageContent(imageUrl!));
        }
        else
        {
            _chatView.ChatState.AddUserMessage(userInputRequest.ChatInput);
        }
        var response = NovelWriterService.ExecuteEditorAgentChat(_chatView.GetChatHistory());
        await HandleStreaming(response);
        _isBusy = false;
        StateHasChanged();
    }

    private async Task HandleStreaming(IAsyncEnumerable<string> response)
    {
        await foreach (var message in response)
        {
            _chatView.ChatState.UpsertAssistantMessage(message);
            await InvokeAsync(StateHasChanged);
        }
    }
}
