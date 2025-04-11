using Lumen.Core.Application;
using Lumen.Extensions.DependencyInjection;
using Lumen.MinimalApi.Models;

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
    .UseSse();
}

app.MapPost("/send-message", async (
    Message message,
    ISseBuilder sse)
    => await sse.Build()
        .SetEvent("new_message")
        .SetData(message)
        .SendToAllClientsAsync());

app.Run();