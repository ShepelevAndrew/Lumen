using Lumen.Core.Application.Configurations;
using Lumen.Core.Application.FluentBuilder;
using Lumen.Core.Application.FluentBuilder.Abstractions;
using Lumen.Core.Application.Services;
using Lumen.Core.Application.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lumen.Extensions.DependencyInjection;

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
            .AddScoped<ISsePublisher, SsePublisher>();
    }
}