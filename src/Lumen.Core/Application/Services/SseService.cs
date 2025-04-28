using System.Collections.Concurrent;
using System.Text;
using Lumen.Core.Application.Configurations;
using Lumen.Core.Application.Services.Abstractions;
using Lumen.Core.Domain;
using Lumen.Core.Domain.ServerMessages;
using Lumen.Core.Domain.ServerMessages.Abstractions;
using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Application.Services;

public class SseService(SseConfig config) : ISseService
{
    private readonly ConcurrentDictionary<SseClientId, SseClient> _clients = new();

    private readonly TimeSpan _connectionMaxLive = config.ConnectionLiveMinutes.HasValue
        ? TimeSpan.FromMinutes(config.ConnectionLiveMinutes.Value)
        : TimeSpan.Zero;

    public IEnumerable<SseClientId> GetSseClients() => _clients.Keys;

    public IEnumerable<SseClientId> GetSseClients(string userId)
        => _clients.Keys.Where(key => key.UserId == userId);

    public void AddClient(SseClient client) => _clients[client.Id] = client;

    public async Task RemoveClient(SseClientId id)
    {
        if (_clients.TryGetValue(id, out var client))
        {
            await client.Response.DisposeAsync();
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

            if (client.ReceivedNotifications > config.MaxNotificationsForConnection)
                break;

            if (client.GetLiveDuration() > _connectionMaxLive && _connectionMaxLive != TimeSpan.Zero)
                break;

            if (config.PingIntervalMilliseconds == 0)
                continue;

            await Task.Delay(config.PingIntervalMilliseconds, ct);
            await SendServerMessage(id, new KeepAliveServerMessage(), ct);
        }
    }

    public bool ClientExists(SseClientId id) => _clients.ContainsKey(id);

    public bool ClientExists(string clientId) => _clients.Keys.Any(key => key.UserId == clientId);

    public async Task SendAsync(
        SseClientId id,
        Notification notification,
        CancellationToken ct = default)
    {
        var sseNotification = notification.ToSseNotification();
        if (Encoding.ASCII.GetByteCount(sseNotification) >= config.MaxNotificationBytes)
        {
            return;
        }

        if (_clients.TryGetValue(id, out var client))
        {
            try
            {
                await client.Response.WriteAsync(sseNotification);
                await client.Response.FlushAsync();

                client.AddReceivedNotification();
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
        => await SendAsync(id, new Notification(null, serverMessage.Event, serverMessage.Data), ct);
}
