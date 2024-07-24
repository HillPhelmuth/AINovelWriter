using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.SignalR.Client;

namespace AINovelWriter.Client;

public class StreamingClient : INovelWriterStreaming
{
    private HubConnection? _hubConnection;
    public event EventHandler<string>? SendTextResponse;
    public event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
    public event Action<string>? SendChapters;
    public event EventHandler<string>? SendChapterText;
    public StreamingClient(NavigationManager navigation)
    {
        var absoluteUri = navigation.ToAbsoluteUri("/streamHub");
        InitHub(absoluteUri);
    }
    private async void InitHub(Uri uri)
    {

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(uri, options =>
            {
                options.HttpMessageHandlerFactory = innerHandler =>
                    new IncludeRequestCredentialsMessageHandler { InnerHandler = innerHandler };
            })
            .Build();

        _hubConnection.On<string>("ReceiveMessage", OnReceiveMessage);
        _hubConnection.On<string>("SendChapters", OnSendChapters);
        _hubConnection.On<string>("SendChapterText", OnSendChapterText);
        _hubConnection.On<ReadOnlyMemory<byte>?>("SendAudio", OnSendAudio);
        await _hubConnection.StartAsync();
    }
    private void OnReceiveMessage(string message)
    {
        SendTextResponse?.Invoke(this, message);
    }
    private void OnSendChapters(string chapters)
    {
        SendChapters?.Invoke(chapters);
    }
    private void OnSendChapterText(string chapterText)
    {
        SendChapterText?.Invoke(this, chapterText);
    }
    private void OnSendAudio(ReadOnlyMemory<byte>? audio)
    {
        SendAudioResponse?.Invoke(this, audio);
    }


    public IAsyncEnumerable<string> WriteNovel(string outline, AIModel aiModel = AIModel.Planner,
        CancellationToken cancellationToken = default)
    {
        return _hubConnection!.InvokeAsync("StartWriting", outline, aiModel, cancellationToken);
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>?> TextToAudioAsync(string text,
        CancellationToken cancellationToken = default)
    {
        return _hubConnection!.InvokeAsync("StartAudio", text, cancellationToken);
    }


}
public class IncludeRequestCredentialsMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}