using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AINovelWriter.Shared.Models;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NovelTone
{
    None,
    [Description("Neutral tone with balanced emotional elements")]
    Neutral,

    [Description("Light-hearted, playful, and amusing tone")]
    Humorous,

    [Description("Serious, somber, or gloomy emotional qualities")]
    Dark,

    [Description("Intense, emotional, and conflict-driven tone")]
    Dramatic,

    [Description("Passionate, tender, and relationship-focused tone")]
    Romantic,

    [Description("Critical, mocking, or ironic tone that exposes flaws")]
    Satirical,

    [Description("Suspenseful, tense, and anxiety-inducing tone")]
    Suspenseful,

    [Description("Mysterious, enigmatic, and puzzle-like tone")]
    Mysterious,

    [Description("Hopeful, positive, and inspiring emotional quality")]
    Uplifting,

    [Description("Melancholic, sad, or mournful tone")]
    Tragic,

    [Description("Intellectual, thought-provoking, and analytical tone")]
    Philosophical,

    [Description("Whimsical, fanciful, and imaginative emotional quality")]
    Whimsical
}