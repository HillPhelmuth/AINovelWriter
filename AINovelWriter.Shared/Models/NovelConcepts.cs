namespace AINovelWriter.Shared.Models;

public class NovelConcepts
{
	public GenreCategory Genre { get; set; }
    public string? GenreDescription => Genre.GetDescription();

    public string? SubGenre { get; set; }
	public List<Genre> SubGenres { get; set; } = [];
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
		        Description: {GenreDescription}
		        Subgenre: {SubGenre}
		        Theme: {Theme}
		        Characters:{Characters}
		        Plot Events: {PlotEvents}
		        """;
	}
}