namespace AINovelWriter.Shared.Models;

public class NovelOutline
{
	public string? Outline { get; set; }
    public AIModel WriterAIModel { get; set; } = AIModel.Grok3;
}