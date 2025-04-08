using Lumen.Core.Application;
using Lumen.Core.Application.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Lumen.Presentation.Extensions.DependencyInjection;

public static class SseServiceCollectionExtensions
{
    public static IServiceCollection AddSse(
        this IServiceCollection services,
        SseConfig config)
    {
        return services
            .AddSingleton(config)
            .AddSingleton<ISseService, SseService>()
            .AddScoped<ISseBuilder, SseSender>();
    }
}