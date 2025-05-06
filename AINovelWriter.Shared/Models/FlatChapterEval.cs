using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using IgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace AINovelWriter.Shared.Models;

public class FlatChapterEval
{
    public int ChapterNumber { get; set; }
    public double CharacterDevelopment { get; set; }
    public double Clarity { get; set; }
    public double Creativity { get; set; }
    public double Engagement { get; set; }
    [JsonProperty("Relevance")]
    public double Style { get; set; }
    public double WritingDetail { get; set; }
    public double Overall => (CharacterDevelopment + Clarity + Creativity + Engagement + Style + WritingDetail) / 6;
    [JsonIgnore]
    [Ignore]
    public string? ChapterText { get; set; }
    public override string ToString()
    {
        return $"Chapter {ChapterNumber}: {Overall}";
    }
}
public class FullNovelEval
{
	public double CharacterDevelopment { get; set; }
	public double Clarity { get; set; }
	public double Creativity { get; set; }
	public double Engagement { get; set; }
    [JsonProperty("Relevance")]
    public double Style { get; set; }
	public double WritingDetail { get; set; }
	public double Overall => (CharacterDevelopment + Clarity + Creativity + Engagement + Style + WritingDetail) / 6;
	public List<FlatChapterEval> ChapterEvals { get; set; } = [];

    public override string ToString()
    {
        // return a markdown table from the list of chapter evaluations
        var table = new StringBuilder();
        table.AppendLine("**Overall Eval Scores**");
		table.AppendLine($"Character Development: {CharacterDevelopment}");
		table.AppendLine($"Clarity: {Clarity}");
		table.AppendLine($"Creativity: {Creativity}");
		table.AppendLine($"Engagement: {Engagement}");
		table.AppendLine($"Style: {Style}");
		table.AppendLine($"Writing Detail: {WritingDetail}");
		table.AppendLine($"Overall: {Overall}");
		table.AppendLine("**Chapter Evals**");
		table.AppendLine("| Chapter | Character Development | Clarity | Creativity | Engagement | Style | Writing Detail | Overall |");
        table.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
        foreach (var chapterEval in ChapterEvals)
        {
            table.AppendLine($"| {chapterEval.ChapterNumber} | {chapterEval.CharacterDevelopment} | {chapterEval.Clarity} | {chapterEval.Creativity} | {chapterEval.Engagement} | {chapterEval.Style} | {chapterEval.WritingDetail} | {chapterEval.Overall} |");
        }
        return table.ToString();
    }
}