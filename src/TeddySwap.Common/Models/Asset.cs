namespace TeddySwap.Common.Models;

public class Asset
{
    public string TxOutputHash { get; init; } = string.Empty;
    public ulong TxOutputIndex { get; init; }
    public TxOutput TxOutput { get; init; } = new TxOutput();
    public string PolicyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ulong Amount { get; set; }
}