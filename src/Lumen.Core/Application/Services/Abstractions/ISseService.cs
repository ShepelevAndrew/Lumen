using Lumen.Core.Domain;
using Lumen.Core.Domain.ServerMessages.Abstractions;
using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Application.Services.Abstractions;

public interface ISseService
{
    public IEnumerable<SseClientId> GetSseClients();

    public IEnumerable<SseClientId> GetSseClients(string userId);

    void AddClient(SseClient client);

    Task RemoveClient(SseClientId id);

    Task ListenAsync(SseClientId id, CancellationToken ct = default);

    bool ClientExists(string userId);

    bool ClientExists(SseClientId id);

    Task SendAsync(
        SseClientId id,
        Notification notification,
        CancellationToken ct = default);

    Task SendServerMessage(
        SseClientId id,
        IServerMessage message,
        CancellationToken ct = default);
}