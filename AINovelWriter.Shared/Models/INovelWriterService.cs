namespace AINovelWriter.Shared.Models;

public interface INovelWriterService
{
    
    Task<NovelConcepts> GenerateNovelIdea(NovelGenre genre);

    Task<string> CreateNovelOutline(string theme, string characterDetails = "", string plotEvents = "",
        string novelTitle = "", int chapters = 15, AIModel aIModel = AIModel.Gpt4O);

    
}

public interface ITextToSpeechService
{
	IAsyncEnumerable<ReadOnlyMemory<byte>?> TextToAudioAsync(string text, string voice = "onyx",
		CancellationToken cancellationToken = default);
	event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
    event EventHandler<AudioState>? SendAudioStateUpdate;
}

public interface INovelWriterStreaming : ITextToSpeechService
{
    event Action<string>? SendChapters;
    event EventHandler<string>? SendChapterText;
    event EventHandler<string>? TextToImageUrl;
	IAsyncEnumerable<string> WriteNovel(string outline, AIModel aiModel = AIModel.Gpt4O,
        CancellationToken cancellationToken = default);

	event EventHandler<string>? SendTitle;
    event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
}
public interface INovelWriter : INovelWriterService, INovelWriterStreaming
{
	Task<string> TextToImage(string novelOutline, string imageStyle = "photo-realistic");
}