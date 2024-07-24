using AINovelWriter.Shared.Models;

namespace AINovelWriter.Client;

public class NovelWriterClient : INovelWriterService
{
    private readonly HttpClient _httpClient;

    public NovelWriterClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public event Action<string>? SendChapters;
    public event EventHandler<string>? SendChapterText;
    public Task<NovelOutline> GenerateNovelIdea(NovelGenre genre)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreateNovelOutline(string theme, string characterDetails = "", string plotEvents = "", string novelTitle = "",
        int chapters = 15, AIModel aIModel = AIModel.Planner)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<string> WriteNovel(string outline, AIModel aiModel = AIModel.Planner,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>?> TextToAudioAsync(string text, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}