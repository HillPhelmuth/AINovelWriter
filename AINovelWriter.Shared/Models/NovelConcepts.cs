namespace AINovelWriter.Shared.Models;

public class NovelConcepts
{
	public NovelGenre Genre { get; set; }
    public string? Theme { get; set; }
    public string? Characters { get; set; }
    public string? PlotEvents { get; set; }
    public string? Title { get; set; }
    public int ChapterCount { get; set; } = 5;
    public AIModel OutlineAIModel { get; set; }
	public override string ToString()
	{
		return $"""
		        Title: {Title}
		        Genre: {Genre.ToString()}
		        Theme: {Theme}
		        Characters:{Characters}
		        Plot Events: {PlotEvents}
		        """;
	}
}
public enum NovelGenre
{
	None,
	Fantasy,
	ScienceFiction,
	Mystery,
	Romance,
	Thriller,
	HistoricalFiction,
	Horror,
	YoungAdult,
	LiteraryFiction,
	Dystopian,
	Adventure,
	Crime,
	MagicalRealism,
	Contemporary,
	Western
}