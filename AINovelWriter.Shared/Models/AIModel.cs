﻿using System.ComponentModel;

namespace AINovelWriter.Shared.Models;

public enum AIModel
{
    [Description("Select a model")]
    None,
    [ModelName("gpt-4o-mini")]
    [AzureOpenAIModel("gpt-35-turbo")]
    [Description("Latest gpt-4o-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
	Gpt4Mini,
	[ModelName("gpt-4o-2024-08-06")]
    [AzureOpenAIModel("gpt-4o")]
    [Description("Latest gpt-4o")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
	Gpt4O,
	[ModelName("chatgpt-4o-latest")]
	[Description("Dynamic gpt-4o")]
	[ModelProvidor("OpenAI")]
	Gpt4OChatGptLatest,//ft:gpt-4o-mini-2024-07-18:hillphelmuth:novel-gen-mini:A138kqIT
    //ft:gpt-4o-mini-2024-07-18:hillphelmuth:novel-gen2-1:A7ylbj6Q
    [ModelName("ft:gpt-4o-mini-2024-07-18:hillphelmuth:novel-gen-fixed:AA4IWyZr")]
    [Description("Latest Fine-tuned gpt-4o-mini")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4MiniFineTuned,
    [ModelName("gpt-3.5-turbo")]
    [AzureOpenAIModel("gpt-35-turbo")]
    [Description("Latest gpt-3.5-turbo")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt35Turbo,
    [ModelName("gpt-4-turbo")]
    [AzureOpenAIModel("gpt-4")]
    [Description("Latest gpt-4-turbo")]
    [ModelProvidor("OpenAI")]
    [ModelProvidor("AzureOpenAI")]
    Gpt4Turbo,
    [ModelName("gemini-pro")]
    [Description("Latest Gemini 1.0 Pro")]
    [ModelProvidor("GoogleAI")]
	Gemini10,
    [ModelName("gemini-1.5-pro-002")]
    [Description("Latest Gemini 1.5 Pro")]
    [ModelProvidor("GoogleAI")]
	Gemini15,
	[ModelName("gemini-1.5-pro-exp-0827")]
	[Description("Experimental Gemini 1.5 Pro")]
	[ModelProvidor("GoogleAI")]
	Gemini15Exp,
    [ModelName("gemini-1.5-flash")]
    [Description("Gemeni Flash")]
    [ModelProvidor("GoogleAI")]
    GeminiFlash,
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
    [ModelName("Local Model")]
    [Description("LM Studio loaded Model")]
    [ModelProvidor("OpenAI")]
    LocalModel

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