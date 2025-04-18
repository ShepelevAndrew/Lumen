using System.Text;

namespace Lumen.Core.Domain;

public sealed class Notification(
    string? id,
    string? eventName,
    string data,
    uint? retry = null
)
{
    public string ToSseNotification() => ToString();

    public override string ToString()
    {
        var sse = new StringBuilder();

        if (id is not null)
            sse.AppendLine($"id: {id}");

        if (eventName is not null)
            sse.AppendLine($"event: {eventName}");

        if (retry is not null)
            sse.AppendLine($"retry: {retry}");

        sse.AppendLine($"data: {data}\n");

        return sse.ToString();
    }
}
