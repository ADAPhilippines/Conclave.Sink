namespace TeddySwap.Common.Models;

public partial record CollateralTxIn
{
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; set; } = new();
    public string TxOutputHash { get; init; } = string.Empty;
    public ulong TxOutputIndex { get; init; }
}