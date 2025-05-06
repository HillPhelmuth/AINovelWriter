using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AINovelWriter.Shared.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NovelAudience
{
    None,
    [Description("General audience with no specific age or demographic targeting")]
    General,

    [Description("Young children ages 5-8, with simple language and concepts")]
    ChildrenEarly,

    [Description("Children ages 8-12, with more complex plots but age-appropriate themes")]
    MiddleGrade,

    [Description("Teenagers ages 13-18, addressing adolescent themes and experiences")]
    YoungAdult,

    [Description("Readers in early adulthood (18-25), with themes of identity and life transitions")]
    EarlyAdult,

    [Description("Mature readers, with sophisticated themes and complex plots")]
    Adult,

    [Description("Readers seeking explicit content, including swearing/cussing, graphic violence and/or very sexual themes")]
    VeryAdult,

    [Description("Academic or specialized readers with scholarly or technical interests")]
    Academic,

    [Description("Family-oriented with content suitable for readers of multiple ages")]
    Family,

    [Description("Readers seeking content that aligns with specific cultural or community values")]
    Cultural,

    [Description("Educational content designed for learning or instruction")]
    Educational
}