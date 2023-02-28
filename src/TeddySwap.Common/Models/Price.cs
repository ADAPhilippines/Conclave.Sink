using System.Numerics;

namespace TeddySwap.Common.Models;

public record Price
{
    public string Hash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public Order Order { get; init; } = new();
    public decimal PriceX { get; init; }
    public decimal PriceY { get; init; }
}