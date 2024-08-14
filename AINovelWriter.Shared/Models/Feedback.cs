using System.Text.Json.Serialization;

namespace AINovelWriter.Shared.Models;

public class Feedback
{
    [JsonPropertyName("Strengths")]
    public string Strengths { get; set; }

    [JsonPropertyName("Weaknesses")]
    public string Weaknesses { get; set; }

    [JsonPropertyName("Suggestions")]
    public string Suggestions { get; set; }
}