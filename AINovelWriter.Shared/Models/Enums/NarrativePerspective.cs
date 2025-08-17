using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NarrativePerspective
{
    None,
    [Prompt("First Person: The narrator speaks as 'I', offering intimate access to their thoughts and feelings.")]
    [Description("First Person")]
    FirstPerson,

    [Prompt("Second Person: The narrator speaks as 'you', placing the reader directly in the protagonist’s role.")]
    [Description("Second Person")]
    SecondPerson,

    [Prompt("Third Person Limited: The narrator follows one character closely, revealing only their thoughts and perceptions.")]
    [Description("Third Person Limited")]
    ThirdPersonLimited,

    [Prompt("Third Person Omniscient: The narrator knows everything, able to reveal thoughts, feelings, and events across characters.")]
    [Description("Third Person Omniscient")]
    ThirdPersonOmniscient,

    [Prompt("Third Person Objective: A 'fly on the wall' perspective—only describing external actions and dialogue without inner thoughts.")]
    [Description("Third Person Objective")]
    ThirdPersonObjective,

    [Prompt("Multiple/Alternating POV: The story shifts between perspectives of different characters across chapters or sections.")]
    [Description("Multiple/Alternating POV")]
    MultiplePOV,

    [Prompt("Collective First Person: A group voice narrates as 'we', creating a chorus-like effect.")]
    [Description("Collective First Person")]
    CollectiveFirstPerson,

    [Prompt("Non-Human Narrator: The story is told from the viewpoint of an object, animal, or abstract entity.")]
    [Description("Non-Human Narrator")]
    NonHumanNarrator
}