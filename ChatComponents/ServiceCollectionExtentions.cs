using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace ChatComponents;

public static class ServiceCollectionExtentions
{
    public static IServiceCollection AddChat(this IServiceCollection services)
    {
        return services.AddScoped<ChatStateCollection>().AddTransient<AppJsInterop>().AddRadzenComponents();
    }
}