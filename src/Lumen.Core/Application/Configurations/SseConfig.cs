namespace Lumen.Core.Application.Configurations;

public class SseConfig
{
    public int PingIntervalMilliseconds { get; set; } = 60_000;

    public int? ConnectionLiveMinutes { get; set; }

    public int? MaxEventsForConnection { get; set; }
};