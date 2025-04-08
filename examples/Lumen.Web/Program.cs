using Lumen.Core.Application;
using Lumen.Core.Application.Configurations;
using Lumen.Presentation.Extensions.DependencyInjection;
using Lumen.Web.Models;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    builder.Services.AddSse(config: new SseConfig(
        PingIntervalMilliseconds: 10_000,
        ConnectionLiveMinutes: 5,
        MaxEventsForConnection: 10));
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    app.UseSse(configure: options =>
    {
        options.ConnectionPath = "/sse/connection";

        options.UserId = ctx =>
        {
            var userIdQuery = ctx.Request.Query["userId"].FirstOrDefault() ?? string.Empty;
            return Guid.Parse(userIdQuery);
        };

        options.DeviceId = ctx =>
        {
            var deviceIdQuery = ctx.Request.Query["deviceId"].FirstOrDefault();
            return Guid.TryParse(deviceIdQuery, out var id) ? id : null;
        };
    });
}

app.MapPost("/send-message/{clientId:guid}", async (
    Guid clientId,
    Guid? deviceId,
    Message message,
    ISseBuilder sse) =>
{
    var sender = sse.Build()
        .SetEventName("new_message")
        .SetData(message);

    if(deviceId != null)
        await sender.SendAsync(clientId, deviceId.Value);
    else
        await sender.SendToClientAllDevicesAsync(clientId);
});

app.Run();