using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AINovelWriter.Shared.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;

namespace AINovelWriter.Shared.Services;

public class TextToSpeechService : ITextToSpeechService
{
	static string speechKey;
	static string speechRegion;
	public TextToSpeechService(IConfiguration configuration)
	{
		speechKey = configuration["AzureAISpeech:ApiKey"]!;
		speechRegion = configuration["AzureAISpeech:Region"]!;
	}
	public async IAsyncEnumerable<ReadOnlyMemory<byte>?> TextToAudioAsync(string chapterText, string voice = "en-US-AvaMultilingualNeural", [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

		// The neural multilingual voice can speak different languages based on the input text.
		speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
		speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);
		using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
		speechSynthesizer.Synthesizing += SpeechSynthesizer_Synthesizing;
		speechSynthesizer.SynthesisStarted += SpeechSynthesizer_SynthesisStarted;
		speechSynthesizer.SynthesisCompleted += SpeechSynthesizer_SynthesisCompleted;
		// Get text from the console and synthesize to the default speaker.
		Console.WriteLine("Enter some text that you want to speak >");
		string text = chapterText;
		foreach (var segment in text.SplitText(int.MaxValue))
		{
			var speechSynthesisResult = await speechSynthesizer.StartSpeakingTextAsync(segment);
			LogSpeechSynthesisResult(speechSynthesisResult, segment);
			var data = new ReadOnlyMemory<byte>(speechSynthesisResult.AudioData);
			yield return data;
		}
		await Task.Delay(TimeSpan.FromSeconds(10));
		SendAudioStateUpdate?.Invoke(this, AudioState.Completed);

	}

	private void SpeechSynthesizer_SynthesisCompleted(object? sender, SpeechSynthesisEventArgs e)
	{
		SendAudioStateUpdate?.Invoke(this, AudioState.Completed);
		Console.WriteLine($"Audio State: {AudioState.Completed}");
	}

	private void SpeechSynthesizer_SynthesisStarted(object? sender, SpeechSynthesisEventArgs e)
	{
		SendAudioStateUpdate?.Invoke(this, AudioState.Started);
		Console.WriteLine($"Audio State: {AudioState.Started}");
	}

	public event EventHandler<ReadOnlyMemory<byte>?>? SendAudioResponse;
	public event EventHandler<AudioState>? SendAudioStateUpdate;

	private void SpeechSynthesizer_Synthesizing(object? sender, SpeechSynthesisEventArgs e)
	{
		SendAudioStateUpdate?.Invoke(this, AudioState.Active);
		Console.WriteLine($"Audio State: {AudioState.Active}");
		var data = e.Result.AudioData;
		var duration = e.Result.AudioDuration;
		Console.WriteLine($"Duration: {duration}");
		SendAudioResponse?.Invoke(this, new ReadOnlyMemory<byte>(data));
	}

	static void LogSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
	{
		switch (speechSynthesisResult.Reason)
		{
			case ResultReason.SynthesizingAudioCompleted:
				Console.WriteLine($"Speech synthesized for text: [{text}]");
				break;
			case ResultReason.Canceled:
				var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
				Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

				if (cancellation.Reason == CancellationReason.Error)
				{
					Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
					Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
					Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
				}
				break;
			default:
				break;
		}
	}

}