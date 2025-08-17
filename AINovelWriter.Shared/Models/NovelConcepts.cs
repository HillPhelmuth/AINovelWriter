using AINovelWriter.Shared.Models.Enums;

namespace AINovelWriter.Shared.Models;

public class NovelConcepts
{
	public GenreCategory Genre { get; set; }
    public string? GenreDescription => Genre.GetDescription();
    

	public List<Genre> SubGenres { get; set; } = [];
    public string? Theme { get; set; }
    public NovelTone Tone { get; set; } = NovelTone.None;
    public NovelAudience Audience { get; set; } = NovelAudience.None;
    public NovelLength Length { get; set; } = NovelLength.None;
    public NarrativePerspective NarrativePerspective { get; set; } = NarrativePerspective.ThirdPersonLimited;
    public string? Description => $"Genre:\n{Genre.ToString() + "\n" + Genre.GetDescription()}\n\n{(SubGenres.Count > 0 ? "Subgenres:\n" + string.Join("\n", SubGenres.Select(x => x.ToString())) : string.Empty)}\nTheme/Description: {Theme}";
    public string? Characters { get; set; }
    public string? PlotEvents { get; set; }
    public string? Title { get; set; }
    public string? AdditionalInstructions {get; set; }
    public int ChapterCount { get; set; } = 5;
    public AIModel OutlineAIModel { get; set; } = AIModel.GeminiFlash;
    public WritingStyle WritingStyle { get; set; }
    public override string ToString()
	{
        return $"""
		        Title: {Title}
		        Genre: {Genre.ToString()} - {Genre.GetDescription()}
		        {(SubGenres.Count > 0 ? "Subgenres: " + string.Join("\n", SubGenres.Select(x => x.ToString())) : string.Empty)}
		        Theme: {Theme}
		        Tone: {Tone.ToString()} - {Tone.GetDescription()}
		        Audience: {Audience.ToString()} - {Audience.GetDescription()}
		        Writing Style: {WritingStyle.ToString()} - {WritingStyle.GetDescription()}
		        Perspective: {NarrativePerspective.GetPromptText()}
		        Characters:{Characters}
		        Primary Plot Events: {PlotEvents}
		        """;
	}

    public string AvailableInformation()
    {
        var sections = new List<string>();

        if (!string.IsNullOrWhiteSpace(Title))
            sections.Add($"Title: {Title}");

        if (Genre != GenreCategory.None)
            sections.Add($"Genre: {Genre.ToString()} - {Genre.GetDescription()}");

        if (SubGenres.Count > 0)
            sections.Add($"Subgenres: {string.Join(", ", SubGenres.Select(x => x.ToString()))}");

        if (!string.IsNullOrWhiteSpace(Theme))
            sections.Add($"Theme: {Theme}");

        if (Tone != NovelTone.None)
            sections.Add($"Tone: {Tone.ToString()} - {Tone.GetDescription()}");

        if (Audience != NovelAudience.None)
            sections.Add($"Audience: {Audience.ToString()} - {Audience.GetDescription()}");

        if (!string.IsNullOrWhiteSpace(Characters))
            sections.Add($"Characters: {Characters}");

        if (!string.IsNullOrWhiteSpace(PlotEvents))
            sections.Add($"Primary Plot Events: {PlotEvents}");

        if (!string.IsNullOrWhiteSpace(AdditionalInstructions))
            sections.Add($"Additional Instructions: {AdditionalInstructions}");

        if (Length != NovelLength.None)
            sections.Add($"Chapter Count: {Length.GetDescription()}");

        return string.Join("\n", sections);
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