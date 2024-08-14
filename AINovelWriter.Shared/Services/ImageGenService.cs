using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AINovelWriter.Shared.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenAI.Images;
using Polly;

namespace AINovelWriter.Shared.Services;

public class ImageGenService(IConfiguration configuration, BlobServiceClient blobServiceClient)
{
    private readonly ImageClient _imageClient = new("dall-e-3", configuration["OpenAI:ApiKey"]!);
    private const string ImageContainerUrl = "https://novelcoverimages.z21.web.core.windows.net";
    private const string ImageDataPrefix = "data:image/png;base64,";
    private readonly BlobContainerClient _blobContainerClient = blobServiceClient.GetBlobContainerClient("$web");

    public async Task<string> GenerateImage(NovelInfo novelInfo, string imageStyle = "photo-realistic", bool isVivid = false)
    {
        var novelOutline = novelInfo.Outline;
        var options = new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.Standard,
            Size = GeneratedImageSize.W1024xH1024,
            Style = isVivid ? GeneratedImageStyle.Vivid : GeneratedImageStyle.Natural,
            ResponseFormat = GeneratedImageFormat.Bytes
        };
        var prompt = await GenerateImagePrompt(novelInfo.ConceptDescription, imageStyle);
        GeneratedImage image = await _imageClient.GenerateImageAsync(prompt + $"\n\n{imageStyle}", options);
        var bytes = image.ImageBytes;

        var imageUrl = $"{ImageDataPrefix}{Convert.ToBase64String(bytes.ToArray())}";

        return imageUrl;
    }
    public async Task<string> SelectImage(string userName, string title, string imageUrl)
    {
        // Extract the image data from the URL getting substring of after 'base64,'
        var base64 = imageUrl[(imageUrl.IndexOf("base64,", StringComparison.Ordinal) + 7)..];
        var bytes = Convert.FromBase64String(base64);
        
        var blobName = @$"{ConvertNonAlphaNumericToUnderscore(userName)}\{ConvertNonAlphaNumericToUnderscore(title)}_cover.png";
        return await SaveAsUrl(blobName, bytes);
    }
    //https://novelcoverimages.blob.core.windows.net/$web/hughblows@yahoo.com/The Maloney Family_cover.png
    public async Task<Stream> GetImageBlob(string imageUrl)
    {
        var blobName = imageUrl.Replace(ImageContainerUrl, "").TrimStart(['/','\\']);
        Console.WriteLine($"Downloading {blobName}");
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        var response = await blobClient.OpenReadAsync();
       
        //using var ms = new MemoryStream();
        //await resultStream.CopyToAsync(ms);
        //var result = ms.ToArray();
        //var bytes = new byte[response.Value.ContentLength];
        //var length = await response.Value.Content.ReadAsync(bytes);
        //Console.WriteLine($"File {blobName} downloaded. {result.Length} bytes");
        return response;
    }
    public static string ConvertNonAlphaNumericToUnderscore(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? "";

        return Regex.Replace(input, @"[^a-zA-Z0-9]", "_");
    }
    private async Task<string> SaveAsUrl(string fileName, byte[] bytes)
    {
        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true);
        return $"{ImageContainerUrl}/{fileName}";
    }
    public async Task<string> SaveUserImage(string fileName, string dataBase64)
    {
        var blobName = Path.Combine("UserProfileImages", fileName);

        var data = Convert.FromBase64String(dataBase64);
        return await SaveAsUrl(blobName, data);

    }
    private async Task<string> GenerateImagePrompt(string conceptDescription, string imageStyle)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        kernelBuilder.Services.AddSingleton(configuration);
        kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler().Configure(o =>
            {
                o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                o.Retry.BackoffType = DelayBackoffType.Exponential;
                o.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(90) };
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(180);
                o.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(5) };
            });
        });
        var kernel = kernelBuilder
            .AddOpenAITextToImage(configuration["OpenAI:ApiKey"]!, modelId: "dall-e-3")
            .AddOpenAIChatCompletion("gpt-4o-mini", configuration["OpenAI:ApiKey"]!)
            .Build();
        var imagePromptPrompt =
            """
			Generate a prompt for an image generating model to create a {{$imageStyle}} image based on a description of the conceps below. Your response will be passed directly to the image generating model, so do not include any additional text.
			Ensure that the art style is {{ $imageStyle}} so emphasize it in your prompt. 
			DO NOT mention it's a for a novel.
			
			## Novel Concepts
			{{ $novel }}

			## Output
			Limit your prompt to 100 words.
			""";

        var kernelArguments = new KernelArguments { ["novel"] = conceptDescription, ["imageStyle"] = imageStyle };
        var prompt = await kernel.InvokePromptAsync<string>(imagePromptPrompt, kernelArguments);
        Console.WriteLine($"ImageStyle: {imageStyle}\n\nPROMPT:\n\n____________\n\n{prompt}\n\n________________\n\n");
        return prompt;
    }
}