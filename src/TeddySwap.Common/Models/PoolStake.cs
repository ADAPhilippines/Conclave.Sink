using System.Numerics;

namespace TeddySwap.Common.Models;

public class PoolStake
{
    public string PoolId { get; init; } = string.Empty;
    public ulong StakeAmount { get; init; }
}