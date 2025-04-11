using Lumen.Core.Domain.ValueObjects;
using Newtonsoft.Json;

namespace Lumen.Core.Application;

public interface ISseBuilder
{
    IEventNameSetter Build();
}

public interface IEventNameSetter : IDataSetter
{
    IDataSetter SetEvent(string eventName);
}

public interface IDataSetter
{
    ISseSender SetData<T>(T data) where T : class;
}

public interface ISseSender
{
    Task SendAsync(
        SseClientId toClientId,
        CancellationToken ct = default);

    Task SendAsync(
        Guid clientId,
        Guid deviceId,
        CancellationToken ct = default);

    Task SendAsync(
        IEnumerable<SseClientId> toClientIds,
        CancellationToken ct = default);

    Task SendToAllClientsAsync(
        CancellationToken ct = default);

    Task SendToClientAllDevicesAsync(
        Guid toClientId,
        CancellationToken ct = default);
}

public class SseSender(
    ISseService sseService
) : ISseBuilder, IEventNameSetter, ISseSender
{
    private string _message = string.Empty;

    public IEventNameSetter Build()
    {
        _message = string.Empty;

        return this;
    }

    public IDataSetter SetEvent(string eventName)
    {
        _message = $"event: {eventName}\n";

        return this;
    }

    public ISseSender SetData<T>(T data)
        where T : class
    {
        var jsonObject = JsonConvert.SerializeObject(data);

        _message += $"data: {jsonObject}\n\n";

        return this;
    }

    public async Task SendAsync(
        SseClientId toClientId,
        CancellationToken ct = default)
        => await sseService.SendAsync(toClientId, _message, ct);

    public async Task SendAsync(
        Guid clientId,
        Guid deviceId,
        CancellationToken ct = default)
        => await SendAsync(new SseClientId(clientId, deviceId), ct);

    public async Task SendAsync(
        IEnumerable<SseClientId> toClientIds,
        CancellationToken ct = default)
    {
        foreach (var toClientId in toClientIds)
        {
            await SendAsync(toClientId, ct);
        }
    }

    public async Task SendToAllClientsAsync(CancellationToken ct = default)
    {
        var sseClients = sseService.GetSseClients();
        await SendAsync(sseClients, ct);
    }

    public async Task SendToClientAllDevicesAsync(
        Guid toClientId,
        CancellationToken ct = default)
    {
        var sseClients = sseService.GetSseClients(toClientId);
        await SendAsync(sseClients, ct);
    }
}
