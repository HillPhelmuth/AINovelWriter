using System.ComponentModel;

namespace AINovelWriter.Shared.Models;

public enum NovelLength
{
    None,
    [Description("Short Story (1-5 Chapters)")]
    ShortStory,
    [Description("Novella (5-15 Chapters)")]
    Novella,
    [Description("Medium Length (15-30 Chapters)")]
    MediumNovel,
    [Description("Longer Novel (30-50 Chapters)")]
    LongNovel,
    [Description("Epic Novel (More Than 50 Chapters)")]
    EpicNovel

}