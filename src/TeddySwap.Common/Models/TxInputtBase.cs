namespace TeddySwap.Common.Models;

public record TxInputBase
{
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; set; } = new();
    public string TxOutputHash { get; init; } = string.Empty;
    public ulong TxOutputIndex { get; init; }
    public byte? InlineDatum { get; init; }
    public TxOutput TxOutput { get; init; } = new TxOutput();
}