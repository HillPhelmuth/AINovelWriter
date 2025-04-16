namespace AINovelWriter.Shared.Models;

public class NovelConcepts
{
	public GenreCategory Genre { get; set; }
    public string? GenreDescription => Genre.GetDescription();
    public string AuthorStyle { get; set; } = "";

    public string? SubGenre { get; set; }
	public List<Genre> SubGenres { get; set; } = [];
    public string? Theme { get; set; }
    public string? Description => $"Genre:\n{Genre.ToString() + "\n" + Genre.GetDescription()}\n\n{(SubGenres.Count > 0 ? "Subgenres:\n" + string.Join("\n", SubGenres.Select(x => x.ToString())) : string.Empty)}\nTheme/Description: {Theme}";
    public string? Characters { get; set; }
    public string? PlotEvents { get; set; }
    public string? Title { get; set; }
    public string? AdditionalInstructions {get; set; }
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

public class NovelConceptOutput
{
    public required string Theme { get; set; }
    public required string Characters { get; set; }
    public required string PlotEvents { get; set; }
    public required string Title { get; set; }
    public NovelConcepts AsNovelConcepts()
    {
        return new NovelConcepts
        {
            Theme = Theme,
            Characters = Characters,
            PlotEvents = PlotEvents,
            Title = Title
        };
    }
}
/// <summary>
/// Represents a model for novel concepts, providing  data structures for themes, characters, plot events, and titles.
/// </summary>
public class NovelConceptFailover
{
    /// <summary>
    /// Gets or sets the theme of the novel.
    /// </summary>
    public string Theme { get; set; }

    /// <summary>
    /// Gets or sets the characters of the novel as an array of strings.
    /// </summary>
    public string[] Characters { get; set; }

    /// <summary>
    /// Gets or sets the plot events of the novel as an array of strings.
    /// </summary>
    public string[] PlotEvents { get; set; }

    /// <summary>
    /// Gets or sets the title of the novel.
    /// </summary>
    public string Title { get; set; }
    
}
