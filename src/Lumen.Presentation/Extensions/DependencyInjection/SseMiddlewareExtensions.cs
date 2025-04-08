using Lumen.Presentation.Configurations;
using Lumen.Presentation.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Lumen.Presentation.Extensions.DependencyInjection;

public static class SseMiddlewareExtensions
{
    public static IApplicationBuilder UseSse(
        this IApplicationBuilder app,
        Action<SseOptions>? configure = null)
    {
        var options = new SseOptions();
        configure?.Invoke(options);
        return app.UseMiddleware<SseMiddleware>(Options.Create(options));
    }
}