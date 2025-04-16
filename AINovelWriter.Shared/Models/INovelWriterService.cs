using Microsoft.SemanticKernel.ChatCompletion;

namespace AINovelWriter.Shared.Models;

public interface INovelWriterService
{
    
    Task<NovelConcepts> GenerateNovelIdea(GenreCategoryItem genre, List<Genre> subgenres, NovelLength length);

    Task<string> CreateNovelOutline(string theme, string characterDetails = "", string plotEvents = "",
        string novelTitle = "", int chapters = 15, AIModel aIModel = AIModel.Gpt41, string additionalInstructions = "");
    Task<string> CreateNovelOutline(NovelConcepts concepts);

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
    event Action<string>? SendOutline;
    event EventHandler<ChapterEventArgs>? SendChapterText;
    event EventHandler<string>? TextToImageUrl;
	IAsyncEnumerable<string> WriteFullNovel(string outline, string authorStyle = "", AIModel aiModel = AIModel.Gpt41,
		CancellationToken cancellationToken = default);
}
public interface INovelWriter : INovelWriterService, INovelWriterStreaming
{
	Task<string> TextToImage(string novelOutline, string imageStyle = "photo-realistic");
    Task<Feedback> ProvideRewriteFeedback(ChapterOutline chapterOutline, AIModel aiModel = AIModel.GeminiFlash,
        string? additionalInstructions = null);
    Task<string> RewriteChapter(ChapterOutline chapterOutline, Feedback feedback,
        AIModel aiModel = AIModel.GeminiFlash, string? userNotes = null);
    Task<NovelInfo> ReverseEngineerNovel(string epubFileData, string title);
    IAsyncEnumerable<string> ReviewFullNovel(NovelInfo novel, ReviewContext reviewContext,
        AIModel aiModel = AIModel.Gpt41);

    IAsyncEnumerable<string> ExecuteEditorAgentChat(ChatHistory chatHistory, AIModel aiModel = AIModel.Gpt41);
    IAsyncEnumerable<string> ExecuteCharacterAgentChat(ChatHistory chatHistory, AIModel aiModel = AIModel.Gpt41);

    Task<string> CompareTwoChapterVersions(string originalChapter, string revisedChapter,
        AIModel aiModel = AIModel.Gpt41);

    Task<string> RewriteChapter(string original, string feedback, AIModel model = AIModel.Gpt41);
}