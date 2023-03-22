using System.Numerics;

namespace TeddySwap.Common.Models;

public record FisoPoolActiveStake
{
    public ulong EpochNumber { get; init; }
    public string PoolId { get; init; } = string.Empty;
    public BigInteger StakeAmount { get; init; }
}