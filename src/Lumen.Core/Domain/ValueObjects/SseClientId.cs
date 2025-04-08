namespace Lumen.Core.Domain.ValueObjects;

public record SseClientId(
    Guid ClientId,
    Guid? DeviceId = null
);