using System.Security.Claims;
using Lumen.Core.Domain;
using Microsoft.AspNetCore.Http;

namespace Lumen.Configurations;

public class SseOptions
{
    public PathString ConnectionPath { get; set; } = "/sse/connect";

    public Func<HttpContext, Guid> UserId { get; set; } = ctx =>
    {
        var userIdClaim = ctx.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        return userIdClaim is null ? Guid.Empty : Guid.Parse(userIdClaim.Value);
    };

    public Func<HttpContext, Guid?> DeviceId { get; set; } = ctx =>
    {
        if (ctx.Request.Query.TryGetValue("deviceId", out var deviceId))
            return Guid.TryParse(deviceId, out var parsed) ? parsed : null;
        return null;
    };

    public SseClient GetSseClient(HttpContext context)
        => new(UserId(context), DeviceId(context), new StreamWriter(context.Response.Body));
}