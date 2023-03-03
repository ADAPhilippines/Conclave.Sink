using System.Numerics;
using TeddySwap.Common.Enums;

namespace TeddySwap.Common.Models;

public record Order
{
    public string TxHash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public string Blockhash { get; set; } = string.Empty;
    public OrderType OrderType { get; init; }
    public Block Block { get; set; } = new();
    public Price? Price { get; init; }
    public string? PoolDatum { get; init; }
    public string? OrderDatum { get; init; }
    public string UserAddress { get; init; } = string.Empty;
    public string BatcherAddress { get; init; } = string.Empty;
    public string AssetX { get; init; } = string.Empty;
    public string AssetY { get; init; } = string.Empty;
    public string AssetLq { get; init; } = string.Empty;
    public string PoolNft { get; init; } = string.Empty;
    public string OrderBase { get; init; } = string.Empty;
    public BigInteger ReservesX { get; init; }
    public BigInteger ReservesY { get; init; }
    public BigInteger Liquidity { get; init; }
    public BigInteger OrderX { get; init; }
    public BigInteger OrderY { get; init; }
    public BigInteger OrderLq { get; init; }
    public ulong Slot { get; init; }
}