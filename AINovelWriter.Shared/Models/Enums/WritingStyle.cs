using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models.Enums;

public enum WritingStyle
{
    None,
    [Description("Short, declarative sentences. Fast pacing, minimal description—great for thrillers.")]
    ConciseAndPunchy,

    [Description("Rich sensory detail and metaphors. Slower pacing—ideal for literary or epic fantasy.")]
    LyricalAndDescriptive,

    [Description("“Show, don’t tell”: scene-setting like a screenplay, dynamic camera angles.")]
    CinematicAndVisual,

    [Description("Cliffhangers, tight focus on what’s at stake, heartbeat-high pacing.")]
    SuspensefulAndTense,

    [Description("Light tone, playful banter, clever similes—perfect for rom-coms or satire.")]
    HumorousAndWitty,

    [Description("Deep inner monologue, philosophical asides, character thought-streams.")]
    IntrospectiveAndReflective,

    [Description("Bare-bones prose, leave room for reader imagination, stark atmospheres.")]
    MinimalistAndSparse,

    [Description("Flourishing language, complex sentence structures, poetic rhythm.")]
    OrnateAndFlourished,

    [Description("Focus on back-and-forth speech, minimal narrative exposition—ideal for dramas.")]
    DialogueDriven,

    [Description("“In medias res” openings, visceral fight/chase descriptions, rapid-fire verbs.")]
    ActionOriented,

    [Description("Gritty first-person voice, street-smart similes, moral ambiguity.")]
    NoirHardBoiled,

    [Description("Archetypal characters, moral framing, whimsical tone, simple structure.")]
    FairytaleFable
}
