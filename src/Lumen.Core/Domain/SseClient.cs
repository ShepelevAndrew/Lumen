using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Domain;

public class SseClient(SseClientId id, StreamWriter responseWriter)
{
    public SseClientId Id { get; private set; } = id;

    public StreamWriter ResponseWriter { get; private set; } = responseWriter;

    public uint ReceivedNotifications { get; private set; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;

    public void AddReceivedNotification() => ReceivedNotifications++;

    public TimeSpan GetLiveDuration() => DateTime.UtcNow - CreatedTime;
}