namespace Conclave.Sink.Models;

public record ConclaveSinkSettings
{
    public ulong EpochLength { get; init; }
}