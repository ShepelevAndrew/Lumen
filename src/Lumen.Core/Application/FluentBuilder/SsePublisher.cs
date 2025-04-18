using Lumen.Core.Application.FluentBuilder.Abstractions;
using Lumen.Core.Application.Services.Abstractions;
using Lumen.Core.Domain;
using Lumen.Core.Domain.ValueObjects;
using Newtonsoft.Json;

namespace Lumen.Core.Application.FluentBuilder;

public class SsePublisher(
    ISseService sseService
) : ISsePublisher, INotificationIdStage, INotificationSender
{
    private string? _id;
    private string? _event;
    private string _data = string.Empty;
    private uint? _retry;

    public INotificationIdStage New() => new SsePublisher(sseService);

    public INotificationRetryStage SetId(string id)
    {
        _id = id;
        return this;
    }

    public INotificationRetryStage SetId(Guid id)
    {
        _id = id.ToString();
        return this;
    }

    public INotificationRetryStage SetId(int id)
    {
        _id = id.ToString();
        return this;
    }

    public INotificationEventStage SetRetry(uint retry)
    {
        _retry = retry;
        return this;
    }

    public INotificationDataStage SetEvent(string eventName)
    {
        _event = eventName;
        return this;
    }

    public INotificationSender SetData<T>(T data)
        where T : class
    {
        var jsonObject = JsonConvert.SerializeObject(data);

        _data = jsonObject;
        return this;
    }

    public async Task SendAsync(
        SseClientId id,
        CancellationToken ct = default)
        => await sseService.SendAsync(id, new Notification(_id, _event, _data, _retry), ct);

    public async Task SendAsync(
        IEnumerable<SseClientId> ids,
        CancellationToken ct = default)
    {
        foreach (var id in ids)
        {
            await SendAsync(id, ct);
        }
    }

    public async Task SendToClientAllDevicesAsync(Guid userId, CancellationToken ct = default)
        => await SendToClientAllDevicesAsync(userId.ToString(), ct);

    public async Task SendToClientAllDevicesAsync(
        string userId,
        CancellationToken ct = default)
    {
        var sseClients = sseService.GetSseClients(userId);
        await SendAsync(sseClients, ct);
    }

    public async Task SendToAllClientsAsync(
        CancellationToken ct = default)
    {
        var sseClients = sseService.GetSseClients();
        await SendAsync(sseClients, ct);
    }
}
