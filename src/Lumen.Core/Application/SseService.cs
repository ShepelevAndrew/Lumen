using System.Collections.Concurrent;
using Lumen.Core.Application.Configurations;
using Lumen.Core.Domain;
using Lumen.Core.Domain.ServerMessages;
using Lumen.Core.Domain.ServerMessages.Abstractions;
using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Application;

public interface ISseService
{
    public IEnumerable<SseClientId> GetSseClients(Guid clientId);

    void AddClient(SseClient client);

    Task RemoveClient(SseClientId id);

    Task ListenAsync(
        SseClientId id,
        CancellationToken ct);

    bool ClientExists(Guid clientId);

    bool ClientExists(SseClientId id);

    Task SendAsync(
        SseClientId id,
        string? eventName,
        string eventData,
        CancellationToken ct = default);

    Task SendAsync(
        SseClientId id,
        string message,
        CancellationToken ct = default);

    Task SendServerMessage(
        SseClientId id,
        IServerMessage message,
        CancellationToken ct = default);
}

public class SseService(SseConfig config) : ISseService
{
    private readonly ConcurrentDictionary<SseClientId, SseClient> _clients = new();

    private readonly TimeSpan _connectionMaxLive = config.ConnectionLiveMinutes.HasValue
        ? TimeSpan.FromMinutes(config.ConnectionLiveMinutes.Value)
        : TimeSpan.Zero;

    public IEnumerable<SseClientId> GetSseClients(Guid clientId)
        => _clients.Keys.Where(key => key.ClientId == clientId);

    public void AddClient(SseClient client) => _clients[client.Id] = client;

    public async Task RemoveClient(SseClientId id)
    {
        if (_clients.TryGetValue(id, out var client))
        {
            await client.ResponseWriter.DisposeAsync();
            _clients.Remove(id, out _);
        }
    }

    public async Task ListenAsync(
        SseClientId id,
        CancellationToken ct)
    {
        await SendServerMessage(id, new ConnectedServerMessage(), ct);

        while (_clients.TryGetValue(id, out var client))
        {
            ct.ThrowIfCancellationRequested();

            if(client.ReceivedEvents > config.MaxEventsForConnection)
                break;

            if(client.GetLiveDuration() > _connectionMaxLive && _connectionMaxLive != TimeSpan.Zero)
                break;

            if (config.PingIntervalMilliseconds == 0)
                continue;

            await Task.Delay(config.PingIntervalMilliseconds, ct);
            await SendServerMessage(id, new KeepAliveServerMessage(), ct);
        }
    }

    public bool ClientExists(SseClientId id) => _clients.ContainsKey(id);

    public bool ClientExists(Guid clientId) => _clients.Keys.Any(key => key.ClientId == clientId);

    public async Task SendAsync(
        SseClientId id,
        string? eventName,
        string eventData,
        CancellationToken ct = default)
    {
        var eventMessage = $"event: {eventName}\ndata: {eventData}\n\n";
        await SendAsync(id, eventMessage, ct);
    }

    public async Task SendAsync(
        SseClientId id,
        string message,
        CancellationToken ct = default)
    {
        if (_clients.TryGetValue(id, out var client))
        {
            try
            {
                await client.ResponseWriter.WriteAsync(message);
                await client.ResponseWriter.FlushAsync(ct);

                client.AddReceivedEvent();
            }
            catch
            {
                await RemoveClient(id);
            }
        }
    }

    public async Task SendServerMessage(
        SseClientId id,
        IServerMessage serverMessage,
        CancellationToken ct = default)
        => await SendAsync(id, serverMessage.Event, serverMessage.Data, ct);
}
