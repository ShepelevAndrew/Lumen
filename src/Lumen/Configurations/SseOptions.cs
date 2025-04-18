using Lumen.Core.Domain;
using Lumen.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Lumen.Configurations;

public class SseOptions
{
    public PathString ConnectionPath { get; set; } = "/sse/connection";

    public Func<HttpContext, string> UserId { get; set; } = _ => Guid.NewGuid().ToString();

    public Func<HttpContext, string?> DeviceId { get; set; } = _ => Guid.NewGuid().ToString();

    public SseClient GetSseClient(HttpContext context)
        => new(new SseClientId(UserId(context), DeviceId(context)), new StreamWriter(context.Response.Body));
}