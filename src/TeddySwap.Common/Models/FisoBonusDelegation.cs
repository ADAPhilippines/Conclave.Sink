namespace TeddySwap.Common.Models;

public record FisoBonusDelegation
{
    public ulong EpochNumber { get; init; }
    public string StakeAddress { get; init; } = string.Empty;
    public string PoolId { get; init; } = string.Empty;
    public string TxHash { get; init; } = string.Empty;
    public ulong Slot { get; init; }

}