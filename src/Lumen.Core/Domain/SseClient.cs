using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Domain;

public class SseClient
{
    public SseClient(SseClientId id, StreamWriter responseWriter)
    {
        Id = id;
        ResponseWriter = responseWriter;
    }

    public SseClient(Guid clientId, Guid? deviceId, StreamWriter responseWriter)
    {
        Id = new SseClientId(clientId, deviceId);
        ResponseWriter = responseWriter;
    }

    public SseClientId Id { get; private set; }

    public StreamWriter ResponseWriter { get; private set; }

    public uint ReceivedEvents { get; private set; }

    public DateTime CreatedTime { get; } = DateTime.UtcNow;

    public void AddReceivedEvent() => ReceivedEvents++;

    public TimeSpan GetLiveDuration() => DateTime.UtcNow - CreatedTime;
}