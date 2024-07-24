using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AINovelWriter.Hubs;

public class StreamHub : Hub
{
    private readonly INovelWriterService _novelWriterService;
    private readonly INovelWriterStreaming _novelWriterStreaming;
    public StreamHub(INovelWriterService aINovelWriter, INovelWriterStreaming novelWriterStreaming)
    {
        _novelWriterService = aINovelWriter;
        _novelWriterStreaming = novelWriterStreaming;
        _novelWriterStreaming.SendChapterText += (sender, text) => Clients.Caller.SendAsync("SendChapterText", text);
        _novelWriterStreaming.SendChapters += chapters => Clients.Caller.SendAsync("SendChapters", chapters);
        _novelWriterStreaming.SendTextResponse += (sender, message) => Clients.Caller.SendAsync("ReceiveMessage", message);
        _novelWriterStreaming.SendAudioResponse += (sender, audio) => Clients.Caller.SendAsync("SendAudio", audio);
    }

    public async Task StartWriting(string outline, AIModel aiModel, CancellationToken cancellationToken)
    {
        await _novelWriterStreaming.WriteNovel(outline, aiModel, cancellationToken: cancellationToken);
    }
    public async Task StartAudio(string text)
    {
        await _novelWriterStreaming.TextToAudioAsync(text);
    }
}