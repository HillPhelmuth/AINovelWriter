using System.ComponentModel;

namespace AINovelWriter.Shared.Models;

public enum AIModel
{
    [Description("Select a model")]
    None,
    [ModelName("gpt-4.1-mini")]
    [AzureOpenAIModel("gpt-4.1-mini")]
    [Description("gpt-4.1-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt41Mini,
    [ModelName("gemini-2.0-flash")]
    [Description("Gemini Flash")]
    [ModelProvidor("GoogleAI")]
    GeminiFlash,
#if DEBUG
    [ModelName("ft:gpt-4o-mini-2024-07-18:hillphelmuth:novel-gen-fixed:AA4IWyZr")]
    [Description("Fine-tuned gpt-4o-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4MiniFineTuned,
#endif
    [ModelName("gpt-4.1")]
    [AzureOpenAIModel("gpt-4.1")]
    [Description("gpt-4.1")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
	Gpt41,
    [ModelName("gpt-4o")]
    [AzureOpenAIModel("gpt-4o")]
    [Description("gpt-4o")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4OCurrent,
	[ModelName("chatgpt-4o-latest")]
	[Description("ChatGPT gpt-4o")]
	[ModelProvidor("OpenAI")]
	Gpt4OChatGptLatest,
    [ModelName("gpt-4.1-nano")]
    [AzureOpenAIModel("gpt-4.1-nano")]
    [Description("gpt-4.1-nano")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt41Nano,
    [ModelName("gpt-4o-mini")]
    [AzureOpenAIModel("gpt-35-turbo")]
    [Description("Latest gpt-4o-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4OMini,
    [ModelName("o3-mini")]
    [Description("o3-mini Reasoning")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    O3Mini,
    [ModelName("o4-mini")]
    [Description("o4-mini Reasoning")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    O4Mini,
    [ModelName("grok-3")]
    [Description("Grok-3")]
    [ModelProvidor("GrokAI")]
	Grok3,
    [ModelName("grok-3-mini")]
    [Description("Grok-3-mini")]
    [ModelProvidor("GrokAI")]
    Grok3Mini,
    [ModelName("grok-2")]
    [Description("Grok 2")]
    [ModelProvidor("GrokAI")]
    Grok2,
    [ModelName("gemini-1.5-pro-002")]
    [Description("Gemini 1.5 Pro")]
    [ModelProvidor("GoogleAI")]
	Gemini15,
	[ModelName("gemini-2.5-pro-preview-06-05")]
	[Description("Gemini 2.5 Pro")]
	[ModelProvidor("GoogleAI")]
	Gemini25Exp,
    [ModelName("gemini-2.5-flash-preview-05-20")]
    [Description("Gemini Flash 2.5")]
    [ModelProvidor("GoogleAI")]
    GeminiFlash25,
#if DEBUG
    [ModelName("Local Model")]
    [Description("LM Studio loaded Model")]
    [ModelProvidor("OpenAI")]
    LocalModel
#endif
}
public class ModelNameAttribute(string model) : Attribute
{
    public string Model { get; set; } = model;
}
public class AzureOpenAIModelAttribute(string model) : Attribute
{
    public string Model { get; set; } = model;
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ModelProvidorAttribute(string providor) : Attribute
{
    public string Providor { get; set; } = providor;
}


[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class PromptAttribute(string promptText) : Attribute
{
    public string PromptText { get; } = promptText;
}

public enum NovelWriterModules
{
    GenerateOutline,
    GenerateChapter,
    ModifyOutline,
    ModifyChapter,
    Review,
    Compare
}