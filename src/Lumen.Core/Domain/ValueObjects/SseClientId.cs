namespace Lumen.Core.Domain.ValueObjects;

public record SseClientId(string UserId, string? DeviceId = null)
{
    public SseClientId(Guid userId, Guid? deviceId = null)
        : this(userId.ToString(), deviceId.ToString())
    {
    }

    public SseClientId(Guid userId, string? deviceId = null)
        : this(userId.ToString(), deviceId)
    {
    }
}