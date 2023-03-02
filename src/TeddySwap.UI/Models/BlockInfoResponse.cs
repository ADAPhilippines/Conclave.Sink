namespace TeddySwap.UI.Models.Models;

public record BlockInfoResponse
{
    public ulong? BlockId { get; init; }
    public string? BlockHash { get; init; }
    public ulong? BlockNo { get; init; }
    public ulong? EpochNo { get; init; }
    public ulong? SlotNo { get; init; }
    public string? SlotLeader { get; init; }
    public ulong? TxCount { get; init; }
    public DateTimeOffset Time { get; init; }
}