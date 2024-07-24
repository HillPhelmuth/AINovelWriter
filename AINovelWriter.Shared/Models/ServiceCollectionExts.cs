using AINovelWriter.Shared.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AINovelWriter.Shared.Models;

public static class ServiceCollectionExts
{
	public static IServiceCollection AddNovelWriterServices(this IServiceCollection services, IConfiguration config)
	{
		var cosmosClient = new CosmosClient(config["Cosmos:ConnectionString"]);
		services.AddScoped<INovelWriter, NovelWriterService>().AddScoped<ImageGenService>().AddScoped<AppState>().AddScoped<ITextToSpeechService, TextToSpeechService>().AddSingleton(cosmosClient).AddScoped<CosmosService>();
		return services;
	}
}
public enum AudioState { None, Started, Active, Completed}