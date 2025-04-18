using Lumen.Core.Application.FluentBuilder.Abstractions;
using Lumen.Core.Application.Services.Abstractions;
using Lumen.Core.Domain;
using Lumen.Extensions.DependencyInjection;
using Lumen.MinimalApi.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
    .AddCors(options => options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
    .AddSse();
}

var app = builder.Build();
{
    app
    .UseHttpsRedirection()
    .UseCors("AllowAll")
    .UseSse(options =>
    {
        options.UserId = ctx => new Guid(ctx.Request.Query["id"].First()!);
    });
}

// SEND TO CONCRETE CLIENT OR CONCRETE CLIENT DEVICE
app.MapPost("/send-message/{clientId:guid}", async (
    Guid clientId,
    Guid? deviceId,
    Message message,
    ISsePublisher sse) =>
{
    var sender = sse.New()
        .SetEvent("new_message")
        .SetData(message);

    if (deviceId.HasValue)
    {
        await sender.SendAsync(clientId, deviceId.Value);
    }
    else
    {
        await sender.SendToClientAllDevicesAsync(clientId);
    }
});

// SEND TO ALL SSE CLIENTS
app.MapPost("/send-message-all", async (
    Message message,
    ISsePublisher sse)
    => await sse.New()
        .SetId(Guid.NewGuid())
        .SetRetry(10)
        .SetEvent("new_message")
        .SetData(message)
        .SendToAllClientsAsync());

// SEND TO ALL SSE CLIENTS BY SERVICE
app.MapPost("/send-message-by-service", async (
    Message message,
    ISseService sseService) =>
{
    var notification = new Notification(
        id: Guid.NewGuid().ToString(),
        eventName: "new_message",
        data: JsonConvert.SerializeObject(message));

    var clientIds = sseService.GetSseClients();
    foreach (var clientId in clientIds)
    {
        await sseService.SendAsync(clientId, notification);
    }
});

app.Run();