using Microsoft.SemanticKernel.Text;
using System.Text.Json.Serialization;
using IgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
#pragma warning disable SKEXP0050

namespace AINovelWriter.Shared.Models;

public class NovelInfo
{
    private string? _text;
    public string id { get; set; } = Guid.NewGuid().ToString();
    public DateOnly CreatedOn { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    [JsonIgnore]
    [Ignore]
    public string? AuthorStyle { get; set; }

    public string? User { get; set; }
    public string Title { get; set; } = "";

    [JsonIgnore]
    [Ignore]
    public string Text
    {
        get
        {
            if (ChapterOutlines.Count == 0 && string.IsNullOrEmpty(_text)) return "";

            return _text ??= string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        }
        set => _text = value;
    }

    public string Outline { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string ConceptDescription { get; set; } = "";
    public NovelConcepts? Concepts { get; set; }
    public List<string> TextPages { get; private set; } = [];
    public string TimeToCompletion { get; set; } = "0:00:00";
    [JsonIgnore]
    [Ignore]
    public string ImgHtml => $"<img src='{ImageUrl}' />";
    public List<ChapterOutline> ChapterOutlines { get; set; } = [];
    public List<FlatChapterEval> ChapterEvals { get; set; } = [];
    public FullNovelEval? NovelEval { get; set; }
    public void SplitIntoPagesByWords(int wordPerPage = 200)
    {
        TextPages = StringHelpers.SplitStringIntoPagesByWords(Text, wordPerPage);
    }
    public void SplitIntoPagesByLines(int linesPerPage = 20)
    {
        TextPages = StringHelpers.SplitStringIntoPagesByLines(Text, linesPerPage);
    }
    public void SplitIntoPagesByTokenLines(int tokensPerLine = 40, bool is4o = false)
    {
        TextChunker.TokenCounter delegateTokenCounter = is4o ? StringHelpers.GetTokens200K : StringHelpers.GetTokens;
        var lines = TextChunker.SplitMarkDownLines(Text.Replace('“', '"').Replace('”', '"').Replace(':', '-').Replace("\n\n", "\n<br/>").Replace("##", "\n##"), tokensPerLine, delegateTokenCounter);
        var lines2 = Text.Split("\n\n");
        Console.WriteLine("Lines:");
        //lines.ForEach(Console.WriteLine);
        var chunks = TextChunker.SplitMarkdownParagraphs(lines, 300, tokenCounter: delegateTokenCounter);
        var chunkSizes = chunks.Select(x => delegateTokenCounter(x)).ToList();
        for (var index = 0; index < chunkSizes.Count; index++)
        {
            var chunk = chunkSizes[index];
            Console.WriteLine($"Chunk size, page {index + 1}: {chunk}");
        }

        TextPages = chunks.Select(x => x.Replace("\r\n\"\n", "\"\n")).ToList();
        //var result = new List<string>();
        //for (var i = 0; i < lines.Count; i += linesPerPage)
        //{
        //	result.Add(string.Join(Environment.NewLine, lines.GetRange(i, Math.Min(linesPerPage, lines.Count - i))));
        //}
        //TextPages = result;
    }

    public void SplitIntoPagesFromClient(List<string> pages)
    {
        TextPages = pages;
    }

    [JsonIgnore]
    public bool IsComplete { get; set; }

}

public record ChapterOutline(string Title, string Text, int ChapterNumber)
{
    public int ChapterNumber { get; set; } = ChapterNumber;
    public string Title { get; set; } = Title;
    public string? FullText { get; set; }
    public bool ShowAudio { get; set; }
    public string? Summary { get; set; }
    public int TokenCount => string.IsNullOrEmpty(FullText) ? 0 : StringHelpers.GetTokens200K(FullText);
    public List<EditorNote> EditorNotes { get; } = [];
    public void AddEditorNote(string section, string note)
    {
        EditorNotes.Add(new EditorNote(section, note));
    }

}

public record EditorNote(string Section, string Note);
public class ChapterEventArgs(string chapterText, string chapterSummary) : EventArgs
{
    public string ChapterText { get; set; } = chapterText;
    public string ChapterSummary { get; set; } = chapterSummary;
}