namespace AINovelWriter.Shared.Models;

public class NovelConcepts
{
	public GenreCategory Genre { get; set; }
    public string? GenreDescription => Genre.GetDescription();
    public string AuthorStyle { get; set; } = "";

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
		        Genre: {Genre.ToString()} - {Genre.GetDescription()}
		        {(SubGenres.Count > 0 ? "Subgenres: " + string.Join("\n", SubGenres.Select(x => x.ToString())) : string.Empty)}
		        Theme: {Theme}
		        Characters:{Characters}
		        Primary Plot Events: {PlotEvents}
		        """;
	}
}

public class NovelConceptFailover
{
    public string? Theme { get; set; }
    public string[]? Characters { get; set; }
    public string[]? PlotEvents { get; set; }
    public string? Title { get; set; }
    public NovelConcepts AsNovelConcepts()
    {
        return new NovelConcepts
        {
            Theme = Theme,
            Characters = string.Join("\n", Characters ?? [""]),
            PlotEvents = string.Join("\n", PlotEvents ?? [""]),
            Title = Title
        };
    }
}
