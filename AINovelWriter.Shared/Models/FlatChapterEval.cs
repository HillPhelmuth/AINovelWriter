using System.Text;
using System.Text.Json.Serialization;
using HillPhelmuth.SemanticKernel.LlmAsJudgeEvals;
using Newtonsoft.Json;
using IgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace AINovelWriter.Shared.Models;

public class FlatChapterEval
{
    public int ChapterNumber { get; set; }
    [JsonProperty("NewCharacterDevelopment")]
    public required EvalScore CharacterDevelopment { get; set; } = new();
    [JsonProperty("CharacterDevelopment")]
    public double OldCharacterDevelopment { get; set; }
    [JsonProperty("NewClarity")]
    public required EvalScore Clarity { get; set; } = new();
    [JsonProperty("Clarity")]
    public double OldClarity { get; set; }
    [JsonProperty("NewCreativity")]
    public required EvalScore Creativity { get; set; } = new();
    [JsonProperty("Creativity")]
    public double OldCreativity { get; set; }
    [JsonProperty("NewEngagement")]
    public required EvalScore Engagement { get; set; } = new();
    [JsonProperty("Engagement")]
    public double OldEngagement { get; set; }
    [JsonProperty("Style")]
    public required EvalScore Style { get; set; } = new();
    [JsonProperty("Relevance")]
    public double OldStyle { get; set; }
    [JsonProperty("NewWritingDetail")]
    public required EvalScore WritingDetail { get; set; } = new();
    [JsonProperty("WritingDetail")]
    public double OldWritingDetail { get; set; }

    public Dictionary<string, string> ScoreExplanations => new()
    {
        ["Character Development"] = CharacterDevelopment.Explanation,
        ["Clarity"] = Clarity.Explanation,
        ["Creativity"] = Creativity.Explanation,
        ["Engagement"] = Engagement.Explanation,
        ["Style"] = Style.Explanation,
        ["Writing Detail"] = WritingDetail.Explanation
    };
    public double Overall => (CharacterDevelopment.ProbScore + Clarity.ProbScore + Creativity.ProbScore + Engagement.ProbScore + Style.ProbScore + WritingDetail.ProbScore) / 6;
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
    [JsonProperty("NewCharacterDevelopment")]
    public required EvalScore CharacterDevelopment { get; set; } = new();
    [JsonProperty("CharacterDevelopment")]
    public double OldCharacterDevelopment { get; set; }
    [JsonProperty("NewClarity")]
    public required EvalScore Clarity { get; set; } = new();
    [JsonProperty("Clarity")]
    public double OldClarity { get; set; }
    [JsonProperty("NewCreativity")]
    public required EvalScore Creativity { get; set; } = new();
    [JsonProperty("Creativity")]
    public double OldCreativity { get; set; }
    [JsonProperty("NewEngagement")]
    public required EvalScore Engagement { get; set; } = new();
    [JsonProperty("Engagement")]
    public double OldEngagement { get; set; }
    [JsonProperty("Style")]
    public required EvalScore Style { get; set; } = new();
    [JsonProperty("Relevance")]
    public double OldStyle { get; set; }
    [JsonProperty("NewWritingDetail")]
    public required EvalScore WritingDetail { get; set; } = new();
    [JsonProperty("WritingDetail")]
    public double OldWritingDetail { get; set; }
    public double Overall => (CharacterDevelopment.ProbScore + Clarity.ProbScore + Creativity.ProbScore + Engagement.ProbScore + Style.ProbScore + WritingDetail.ProbScore) / 6;
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
public class EvalScore
{
    public EvalScore(string explanation, double probScore, string evalName)
    {
        this.Explanation = explanation;
        this.ProbScore = probScore;
        this.EvalName = evalName;
    }
    public EvalScore(){}
    public override string ToString()
    {
        return $"{EvalName}: {ProbScore} ({Explanation})";
    }

    public string Explanation { get; set; } = "";
    public double ProbScore { get; set; }
    public string EvalName { get; set; } = "";

    
}

public static class EvalExts
{
    public static EvalScore AsEvalScore(this ResultScore score)
    {
        return new EvalScore(score.Reasoning, score.ProbScore, score.EvalName);
    }
}