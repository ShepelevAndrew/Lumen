namespace Lumen.Core.Application.Configurations;

public class SseConfig
{
    public int PingIntervalMilliseconds { get; set; }

    public int? ConnectionLiveMinutes { get; set; }

    public uint? MaxEventsForConnection { get; set; }

    public uint? MaxNotificationBytes { get; set; }
}