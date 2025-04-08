using Lumen.Core.Application;
using Lumen.Core.Application.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Lumen.Presentation.Extensions.DependencyInjection;

public static class SseServiceCollectionExtensions
{
    public static IServiceCollection AddSse(
        this IServiceCollection services,
        Action<SseConfig>? configure = null)
    {
        var sseConfig = new SseConfig();
        configure?.Invoke(sseConfig);

        return services
            .AddSingleton(sseConfig)
            .AddSingleton<ISseService, SseService>()
            .AddScoped<ISseBuilder, SseSender>();
    }
}