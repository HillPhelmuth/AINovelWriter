using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models;

public class ConfigurationSettings
{
	private IConfiguration _configRoot;
	private static ConfigurationSettings? s_instance;

	public ConfigurationSettings(IConfiguration configRoot)
	{
		_configRoot = configRoot;
	}
	public static void Initialize(IConfiguration configRoot)
	{
		s_instance = new ConfigurationSettings(configRoot);
		//var (isValid, message) = Validate();
		//if (!isValid)
		//	throw new Exception(message);

	}
	private static OpenAIConfig? _openAi;
	public static OpenAIConfig? OpenAI
	{
		get => _openAi ?? LoadSection<OpenAIConfig>();
		set => _openAi = value;
	}
	private static MistralAIConfig? _mistralAI;
	public static MistralAIConfig? MistralAI
	{
		get => _mistralAI ?? LoadSection<MistralAIConfig>();
		set => _mistralAI = value;
	}
	private static GoogleAIConfig? _googleAI;
	public static GoogleAIConfig? GoogleAI
	{
		get => _googleAI ?? LoadSection<GoogleAIConfig>();
		set => _googleAI = value;
	}
    private static AnthropicAIConfig? _anthropicAI;
    public static AnthropicAIConfig? AnthropicAI
    {
        get => _anthropicAI ?? LoadSection<AnthropicAIConfig>();
        set => _anthropicAI = value;
    }
    private static GrokAIConfig? _grokAI;
    public static GrokAIConfig? GrokAI
    {
        get => _grokAI ?? LoadSection<GrokAIConfig>();
        set => _grokAI = value;
    }
    private static T? LoadSection<T>([CallerMemberName] string? caller = null)
	{
		if (s_instance == null)
		{
			throw new InvalidOperationException(
				"ConfigurationSettings must be initialized with a call to Initialize(IConfigurationRoot) before accessing configuration values.");
		}

		if (string.IsNullOrEmpty(caller))
		{
			return default(T);
		}


		return s_instance._configRoot.GetSection(caller).Get<T>() ?? throw new Exception(caller);
	}
}
public class OpenAIConfig
{
	public string ApiKey { get; set; }
	
}
public class GoogleAIConfig
{
	public string ApiKey { get; set; }
}
public class MistralAIConfig
{
	public string ApiKey { get; set; }
}
public class AnthropicAIConfig
{
    public string ApiKey { get; set; }
}
public class GrokAIConfig
{
    public string ApiKey { get; set; }
}
