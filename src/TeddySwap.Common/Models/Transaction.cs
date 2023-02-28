namespace TeddySwap.Common.Models;

public record Transaction
{
    public string Hash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public ulong Fee { get; init; }
    public Block Block { get; init; } = new();
    public IEnumerable<TxInput> Inputs { get; init; } = new List<TxInput>();
    public IEnumerable<CollateralTxInput> CollateralInputs { get; init; } = new List<CollateralTxInput>();
    public IEnumerable<TxOutput> Outputs { get; init; } = new List<TxOutput>();
    public IEnumerable<Order> Orders { get; init; } = new List<Order>();
    public CollateralTxOutput? CollateralOutput { get; init; }
}