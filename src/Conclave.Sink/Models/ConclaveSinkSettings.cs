using CardanoSharp.Wallet.Enums;

namespace Conclave.Sink.Models;

public record ConclaveSinkSettings
{
    public ulong EpochLength { get; init; }
    public NetworkType NetworkType { get; init; }
}