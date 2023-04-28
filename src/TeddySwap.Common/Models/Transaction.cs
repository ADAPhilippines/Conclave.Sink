namespace TeddySwap.Common.Models;

public record Transaction
{
    public string Hash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public ulong Fee { get; init; }
    public string Blockhash { get; init; } = string.Empty;
    public Block Block { get; init; } = new();
    public IEnumerable<TxInput> Inputs { get; init; } = new List<TxInput>();
    public IEnumerable<TxOutput> Outputs { get; init; } = new List<TxOutput>();
    public IEnumerable<CollateralTxIn> CollateralTxIns { get; init; } = new List<CollateralTxIn>();
    public CollateralTxOut? CollateralTxOut { get; init; }
    public bool HasCollateralOutput { get; init; }
    public string? Metadata { get; init; }

}