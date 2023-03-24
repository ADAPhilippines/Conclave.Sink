using System.Numerics;

namespace TeddySwap.Common.Models;

public class Delegator
{
    public string StakeAddress { get; init; } = string.Empty;
    public string PoolId { get; init; } = string.Empty;
    public ulong StakeAmount { get; init; }
    public ulong Epoch { get; init; }
}