using System.ComponentModel;

namespace AINovelWriter.Shared.Models;

public enum AIModel
{
    [Description("Select a model")]
    None,
    [ModelName("gpt-4.1-mini")]
    [AzureOpenAIModel("gpt-4.1-mini")]
    [Description("Nnew gpt-4.1-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt41Mini,
#if DEBUG
    [ModelName("ft:gpt-4o-mini-2024-07-18:hillphelmuth:novel-gen-fixed:AA4IWyZr")]
    [Description("Fine-tuned gpt-4o-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4MiniFineTuned,
#endif
    [ModelName("gpt-4.1")]
    [AzureOpenAIModel("gpt-4.1")]
    [Description("New gpt-4.1")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
	Gpt41,
    [ModelName("gpt-4o")]
    [AzureOpenAIModel("gpt-4o")]
    [Description("Current gpt-4o")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4OCurrent,
	[ModelName("chatgpt-4o-latest")]
	[Description("Dynamic gpt-4o")]
	[ModelProvidor("OpenAI")]
	Gpt4OChatGptLatest,
    [ModelName("gpt-4.1-nano")]
    [AzureOpenAIModel("gpt-4.1-nano")]
    [Description("New gpt-4.1-nano")]
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
    [ModelName("grok-3")]
    [Description("Grok-3")]
    [ModelProvidor("GrokAI")]
	Grok3,
    [ModelName("grok-3-mini")]
    [Description("Grok-3-mini model")]
    [ModelProvidor("GrokAI")]
    Grok3Mini,
    [ModelName("grok-2")]
    [Description("Grok 2 latest")]
    [ModelProvidor("GrokAI")]
    Grok2,
    [ModelName("gemini-1.5-pro-002")]
    [Description("Latest Gemini 1.5 Pro")]
    [ModelProvidor("GoogleAI")]
	Gemini15,
	[ModelName("gemini-2.5-pro-exp-03-25")]
	[Description("Experimental Gemini")]
	[ModelProvidor("GoogleAI")]
	Gemini25Exp,
    [ModelName("learnlm-1.5-pro-experimental")]
    [Description("LearnLM 1.5 Pro Experimental")]
    [ModelProvidor("GoogleAI")]
    GeminiLearnLmExp,
    [ModelName("gemini-2.0-flash")]
    [Description("Gemini Flash")]
    [ModelProvidor("GoogleAI")]
    GeminiFlash,
    [ModelName("gemini-2.0-flash-thinking-exp-01-21")]
    [Description("Gemeni 2.0 Flash - thinking")]
    [ModelProvidor("GoogleAI")]
    Gemini20FlashThinking,
    [ModelName("open-mistral-nemo")]
	[Description("Open Mistral Nemo")]
	[ModelProvidor("MistralAI")]
	OpenMistralNemo,
	[ModelName("open-mixtral-8x7b")]
	[Description("Open Mixtral 8x7B")]
	[ModelProvidor("MistralAI")]
	OpenMixtral8By7B,
	[ModelName("open-mixtral-8x22b")]
	[Description("Open Mixtral 8x22B")]
	[ModelProvidor("MistralAI")]
	OpenMixtral8By22B,
	[ModelName("mistral-small-latest")]
	[Description("Mistral Small Latest")]
	[ModelProvidor("MistralAI")]
	MistralSmallLatest,
	[ModelName("mistral-large-latest")]
	[Description("Mistral Large Latest")]
	[ModelProvidor("MistralAI")]
	MistralLargeLatest,
    [ModelName("anthropic.claude-3-5-sonnet-20241022-v2:0")]
    [Description("Claude Sonnet v2")]
    [ModelProvidor("AnthropicAI")]
    ClaudeSonnet,
    [ModelName("anthropic.claude-3-5-haiku-20241022-v1:0")]
    [Description("Claude Haiku")]
    [ModelProvidor("AnthropicAI")]
    ClaudeHaiku,
    [ModelName("claude-3-5-sonnet-v2@20241022")]
    ClaudeSonnetVertex,
    ClaudeHaikuVertex,
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