using System.ComponentModel;

namespace AINovelWriter.Shared.Models.Enums;

public enum Personality
{
    [Description("Just a standard AI bot"), Temp(0.9f)]
    Default,

    [Description("Showing a lively, bold, and somewhat impudent spirit."), Temp(1.0f)]
    Sassy,

    [Description("Given to or characterized by joking. Funny."), Temp(1.0f)]
    Jokey,

    [Description("Marked by comprehension, empathy, and sympathy."), Temp(0.9f)]
    Understanding,

    [Description("Serving to instruct or inform; conveying instruction, knowledge, or information; enlightening, professorial."), Temp(0.7f)]
    Instructive,

    [Description("Serious, grave, or solemn in manner, tone, or expression."), Temp(0.9f)]
    Somber,

    [Description("Fond of talking in an easy, familiar, and friendly manner; talkative."), Temp(1.0f)]
    Chatty,

    [Description("Mentally quick and resourceful; ingenious."), Temp(1.1f)]
    Clever,

    [Description("Relaxed and tolerant in approach or manner; unhurried and unworried."), Temp(0.9f)]
    EasyGoing,

    [Description("Surly or ill-tempered; grumbling."), Temp(0.9f)]
    Grumpy,

    [Description("Having or showing a friendly, generous, and considerate nature."), Temp(0.7f)]
    Kind,

    [Description("Unpleasantly ill-natured. Nasty"), Temp(0.9f)]
    Mean,

    [Description("Pleasant; agreeable; satisfactory."), Temp(0.7f)]
    Nice,

    [Description("Having or showing polished manners, civility, and breeding."), Temp(0.7f)]
    Polite,

    [Description("Lacking civility or good manners; discourteous; impolite."), Temp(0.9f)]
    Rude,

    [Description("Easily frightened; timid."), Temp(0.7f)]
    Shy,
   
    [Description("Excessively proud of one's appearance or achievements."), Temp(1.1f)]
    Vain,

    [Description("Having or showing good judgment or discernment; wise."), Temp(0.9f)]
    Wise,
    [Description("Overestimates their abilities or importance and displays a sense of superiority or entitlement towards others."), Temp(0.9f)]
    Arrogant,
    [Description("Speaks in a playful or humorous way, often in a manner that is lighthearted and not serious"), Temp(1.1f)]
    Silly,
    [Description("Having or showing a strong desire and determination to succeed."), Temp(0.9f)]
    Passionate,
    [Description("Being characterized by a strong influence on others to excel, perform, or to be creative."), Temp(1.0f)]
    Inspiring,
    [Description("Full of energy and enthusiasm."), Temp(0.9f)]
    Energetic,
    [Description("Curious, eager for knowledge."), Temp(0.9f)]
    Inquisitive,
    [Description("Using logical reasoning, systematic and exact."), Temp(0.6f)]
    Analytical,
    [Description("Showing a natural creative skill, appreciating art and beauty."), Temp(1.1f)]
    Artistic,
    [Description("Being understanding and sensitive to others' feelings."), Temp(0.9f)]
    Empathetic,
    [Description("Able to endure waiting, delay, or provocation without becoming annoyed or upset."), Temp(0.9f)]
    Patient,
   


}

[AttributeUsage(AttributeTargets.Field)]
public class TempAttribute : Attribute
{
    public float Temperature { get; set; }

    public TempAttribute(float temperature)
    {
        Temperature = temperature;
    }
}