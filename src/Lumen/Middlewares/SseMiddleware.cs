using Lumen.Configurations;
using Lumen.Core.Application;
using Lumen.Core.Domain;
using Lumen.Core.Domain.ServerMessages;
using Lumen.Core.Domain.ServerMessages.Abstractions;
using Lumen.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lumen.Middlewares;

public class SseMiddleware(
    RequestDelegate next,
    ISseService sseService,
    IOptions<SseOptions> options)
{
    private readonly SseOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsNotSseConnectionRequest(context))
        {
            await next(context); return;
        }

        var sseClient = _options.GetSseClient(context);

        ConfigureSseHeaders(context);
        await AddSseClientOverrideIfExists(sseClient);

        try
        {
            await sseService.ListenAsync(sseClient.Id, context.RequestAborted);
            await RemoveClientAndSendServerMessage(sseClient.Id, new DisconnectedServerMessage());
        }
        catch (Exception ex)
        {
            await RemoveClientAndSendServerMessage(sseClient.Id, new ErrorServerMessage(ex.Message));
        }
    }

    private bool IsNotSseConnectionRequest(HttpContext context)
        => !string.Equals(context.Request.Path, _options.ConnectionPath, StringComparison.OrdinalIgnoreCase);

    private async Task AddSseClientOverrideIfExists(SseClient sseClient)
    {
        if (sseService.ClientExists(sseClient.Id))
        {
            await sseService.RemoveClient(sseClient.Id);
        }

        sseService.AddClient(sseClient);
    }

    private async Task RemoveClientAndSendServerMessage(
        SseClientId clientId,
        IServerMessage serverMessage)
    {
        await sseService.SendServerMessage(clientId, serverMessage);
        await sseService.RemoveClient(clientId);
    }

    private static void ConfigureSseHeaders(HttpContext context)
    {
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers.Add("Cache-Control", "no-cache");
        context.Response.Headers.Add("X-Accel-Buffering", "no");

        if (context.Request.Protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.Add("Connection", "Keep-Alive");
        }
    }
}