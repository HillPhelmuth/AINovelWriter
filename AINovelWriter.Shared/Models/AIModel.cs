using System.ComponentModel;

namespace AINovelWriter.Shared.Models;

public enum AIModel
{
    [Description("Select a model")]
    None,
    [ModelName("gpt-4.1-mini")]
    [AzureOpenAIModel("gpt-4.1-mini")]
    [Description("gpt-4.1-mini")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt41Mini,
    [ModelName("gemini-2.0-flash")]
    [Description("Gemini Flash")]
    [ModelProvider("GoogleAI")]
    GeminiFlash,
#if DEBUG
    [ModelName("ft:gpt-4o-mini-2024-07-18:hillphelmuth:novel-gen-fixed:AA4IWyZr")]
    [Description("Fine-tuned gpt-4o-mini")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt4MiniFineTuned,
#endif
    [ModelName("gpt-4.1")]
    [AzureOpenAIModel("gpt-4.1")]
    [Description("gpt-4.1")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
	Gpt41,
    [ModelName("gpt-5")]
    [AzureOpenAIModel("gpt-5")]
    [Description("gpt-5")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt5,
    [ModelName("gpt-5-chat-latest")]
    [AzureOpenAIModel("gpt-5-chat")]
    [Description("gpt-5-chat")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt5ChatLatest,
    [ModelName("gpt-4o")]
    [AzureOpenAIModel("gpt-4o")]
    [Description("gpt-4o")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt4OCurrent,
	[ModelName("chatgpt-4o-latest")]
	[Description("ChatGPT gpt-4o")]
	[ModelProvider("OpenAI")]
	Gpt4OChatGptLatest,
    [ModelName("gpt-4.1-nano")]
    [AzureOpenAIModel("gpt-4.1-nano")]
    [Description("gpt-4.1-nano")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt41Nano,
    [ModelName("gpt-4o-mini")]
    [AzureOpenAIModel("gpt-35-turbo")]
    [Description("Latest gpt-4o-mini")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt4OMini,
    [ModelName("gpt-5-mini")]
    [AzureOpenAIModel("gpt5-mini")]
    [Description("gpt-5-mini")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt5Mini,
    [ModelName("gpt-5-nano")]
    [AzureOpenAIModel("gpt-5-nano")]
    [Description("gpt-5-nano")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    Gpt5Nano,
    [ModelName("o3")]
    [Description("o3 Reasoning")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    O3,
    [ModelName("o4-mini")]
    [Description("o4-mini Reasoning")]
    [ModelProvider("OpenAI")]
    [ModelProvider("AzureOpenAI")]
    O4Mini,
    [ModelName("gpt-oss-20b")]
    [Description("gpt-oss-20b")]
    [ModelProvider("OpenAIOss")]
    GptOss20B,
    [ModelName("gpt-oss-120b")]
    [Description("gpt-oss-120b")]
    [ModelProvider("OpenAIOss")]
    GptOss120B,
    [ModelName("grok-4")]
    [Description("Grok-4")]
    [ModelProvider("GrokAI")]
    Grok4,
    [ModelName("grok-3")]
    [Description("Grok-3")]
    [ModelProvider("GrokAI")]
	Grok3,
    [ModelName("grok-3-mini")]
    [Description("Grok-3-mini")]
    [ModelProvider("GrokAI")]
    Grok3Mini,
    [ModelName("grok-2")]
    [Description("Grok 2")]
    [ModelProvider("GrokAI")]
    Grok2,
    [ModelName("gemini-1.5-pro-002")]
    [Description("Gemini 1.5 Pro")]
    [ModelProvider("GoogleAI")]
	Gemini15,
	[ModelName("gemini-2.5-pro-preview-06-05")]
	[Description("Gemini 2.5 Pro")]
	[ModelProvider("GoogleAI")]
	Gemini25Exp,
    [ModelName("gemini-2.5-flash-preview-05-20")]
    [Description("Gemini Flash 2.5")]
    [ModelProvider("GoogleAI")]
    GeminiFlash25,
#if DEBUG
    [ModelName("Local Model")]
    [Description("LM Studio loaded Model")]
    [ModelProvider("OpenAI")]
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
public class ModelProviderAttribute(string provider) : Attribute
{
    public string Provider { get; set; } = provider;
}


[AttributeUsage(AttributeTargets.Field)]
public sealed class PromptAttribute(string promptText) : Attribute
{
    public string PromptText { get; } = promptText;
}
[AttributeUsage(AttributeTargets.Field)]
public sealed class DisplayNameAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
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