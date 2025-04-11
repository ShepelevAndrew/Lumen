using Lumen.Core.Domain;
using Microsoft.AspNetCore.Http;

namespace Lumen.Configurations;

public class SseOptions
{
    public PathString ConnectionPath { get; set; } = "/sse/connection";

    public Func<HttpContext, Guid> UserId { get; set; } = _ => Guid.NewGuid();

    public Func<HttpContext, Guid?> DeviceId { get; set; } = _ => null;

    public SseClient GetSseClient(HttpContext context)
        => new(UserId(context), DeviceId(context), new StreamWriter(context.Response.Body));
}