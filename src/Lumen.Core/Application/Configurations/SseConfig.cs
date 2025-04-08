namespace Lumen.Core.Application.Configurations;

public record SseConfig(
    int PingIntervalMilliseconds = 60_000,
    int? ConnectionLiveMinutes = null,
    int? MaxEventsForConnection = null,
    int? MaxBytesForConnection = null
);