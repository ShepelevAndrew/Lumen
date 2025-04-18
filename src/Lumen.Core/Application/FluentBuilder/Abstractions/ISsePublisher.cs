using Lumen.Core.Domain.ValueObjects;

namespace Lumen.Core.Application.FluentBuilder.Abstractions;

public interface ISsePublisher
{
    INotificationIdStage New();
}

public interface INotificationIdStage : INotificationRetryStage
{
    INotificationRetryStage SetId(string id);

    INotificationRetryStage SetId(Guid id);

    INotificationRetryStage SetId(int id);
}

public interface INotificationRetryStage : INotificationEventStage
{
    INotificationEventStage SetRetry(uint retry);
}

public interface INotificationEventStage : INotificationDataStage
{
    INotificationDataStage SetEvent(string eventName);
}

public interface INotificationDataStage
{
    INotificationSender SetData<T>(T data) where T : class;
}

public interface INotificationSender
{
    Task SendAsync(
        SseClientId id,
        CancellationToken ct = default);

    Task SendAsync(
        Guid clientId,
        Guid deviceId,
        CancellationToken ct = default);

    Task SendAsync(
        IEnumerable<SseClientId> ids,
        CancellationToken ct = default);

    Task SendToClientAllDevicesAsync(
        Guid clientId,
        CancellationToken ct = default);

    Task SendToAllClientsAsync(
        CancellationToken ct = default);
}