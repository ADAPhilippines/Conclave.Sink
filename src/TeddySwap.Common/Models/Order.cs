using System.Numerics;

namespace TeddySwap.Common.Models;

public record Order
{
    public string Hash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public OrderType OrderType { get; init; }
    public string RewardAddress { get; init; } = string.Empty;
    public string BatcherAddress { get; init; } = string.Empty;
    public string AssetX { get; init; } = string.Empty;
    public string AssetY { get; init; } = string.Empty;
    public string AssetLq { get; init; } = string.Empty;
    public string PoolNft { get; init; } = string.Empty;
    public BigInteger ReservesX { get; init; }
    public BigInteger ReservesY { get; init; }
    public BigInteger Liquidity { get; init; }
    public BigInteger OrderX { get; init; }
    public BigInteger OrderY { get; init; }
    public BigInteger OrderLq { get; init; }
    public ulong Slot { get; init; }
}