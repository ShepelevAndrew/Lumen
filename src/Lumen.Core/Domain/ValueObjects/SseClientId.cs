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

    public string UserId { get; } = UserId;

    public string? DeviceId { get; } = DeviceId;

    public static SseClientId Generate() => new(Guid.NewGuid(), Guid.NewGuid());
}