using System.Collections.Concurrent;
using Lumen.Core.Application.Configurations;
using Lumen.Core.Domain;
using Lumen.Core.Domain.Enums;
using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Application;

public interface ISseService
{
    public IEnumerable<SseClientId> GetSseClients(Guid clientId);

    void AddClient(SseClient client);

    Task RemoveClient(SseClientId client);

    Task ListenAsync(
        SseClientId client,
        CancellationToken ct);

    bool ClientExists(SseClientId client);

    Task SendAsync(
        SseClientId toClient,
        string message,
        CancellationToken ct = default);

    Task SendServerMessage(
        SseClientId toClient,
        ServerMessageType message,
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

    public async Task RemoveClient(SseClientId clientId)
    {
        if (_clients.TryGetValue(clientId, out var client))
        {
            await client.ResponseWriter.DisposeAsync();
            _clients.Remove(clientId, out _);
        }
    }

    public async Task SendServerMessage(
        SseClientId toClientId,
        ServerMessageType message,
        CancellationToken ct = default)
    {
        var (serverEventName, serverEventData) = message switch
        {
            ServerMessageType.Connected => ("open", "The server is connected."),
            ServerMessageType.KeepAlive => ("ping", "keep_alive"),
            ServerMessageType.Disconnected => ("disconnect", "The server is shutting down."),
            _ => throw new ArgumentOutOfRangeException(nameof(message), message, null)
        };

        var eventMessage = $"event: {serverEventName}\ndata: {serverEventData}\n\n";

        await SendAsync(toClientId, eventMessage, ct);
    }

    public async Task ListenAsync(
        SseClientId clientId,
        CancellationToken ct)
    {
        await SendServerMessage(clientId, ServerMessageType.Connected, ct);

        while (_clients.TryGetValue(clientId, out var client))
        {
            ct.ThrowIfCancellationRequested();

            if(client.ReceivedEvents > config.MaxEventsForConnection)
                throw new Exception($"Client {clientId} has reached the maximum number of events.");

            if(client.GetLiveDuration() > _connectionMaxLive && _connectionMaxLive != TimeSpan.Zero)
                throw new TimeoutException();

            if (config.PingIntervalMilliseconds == 0)
                continue;

            await Task.Delay(config.PingIntervalMilliseconds, ct);
            await SendServerMessage(clientId, ServerMessageType.KeepAlive, ct);
        }
    }

    public bool ClientExists(SseClientId clientId) => _clients.ContainsKey(clientId);

    public async Task SendAsync(
        SseClientId toClientId,
        string message,
        CancellationToken ct = default)
    {
        if (_clients.TryGetValue(toClientId, out var client))
        {
            try
            {
                await client.ResponseWriter.WriteAsync(message);
                await client.ResponseWriter.FlushAsync(ct);

                client.AddReceivedEvent();
            }
            catch
            {
                await RemoveClient(toClientId);
            }
        }
    }
}
