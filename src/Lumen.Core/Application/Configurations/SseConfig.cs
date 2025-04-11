namespace Lumen.Core.Application.Configurations;

public class SseConfig
{
    public int PingIntervalMilliseconds { get; set; }

    public int? ConnectionLiveMinutes { get; set; }

    public int? MaxEventsForConnection { get; set; }
};