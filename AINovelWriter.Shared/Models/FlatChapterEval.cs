using System.Text.Json.Serialization;
using IgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace AINovelWriter.Shared.Models;

public class FlatChapterEval
{
    public int ChapterNumber { get; set; }
    public double CharacterDevelopment { get; set; }
    public double Clarity { get; set; }
    public double Creativity { get; set; }
    public double Engagement { get; set; }
    public double Relevance { get; set; }
    public double WritingDetail { get; set; }
    public double Overall => (CharacterDevelopment + Clarity + Creativity + Engagement + Relevance + WritingDetail) / 6;
    [JsonIgnore]
    [Ignore]
    public string? ChapterText { get; set; }
}
public class FullNovelEval
{
	public double CharacterDevelopment { get; set; }
	public double Clarity { get; set; }
	public double Creativity { get; set; }
	public double Engagement { get; set; }
	public double Relevance { get; set; }
	public double WritingDetail { get; set; }
	public double Overall => (CharacterDevelopment + Clarity + Creativity + Engagement + Relevance + WritingDetail) / 6;
	public List<FlatChapterEval> ChapterEvals { get; set; } = [];
}