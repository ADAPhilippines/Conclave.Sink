using CardanoSharp.Wallet.Enums;

namespace TeddySwap.Sink.Models;

public record FisoPool
{
    public string PoolId { get; init; } = string.Empty;
    public ulong JoinEpoch { get; init; }
}