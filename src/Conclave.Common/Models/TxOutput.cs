namespace Conclave.Common.Models;

public record TxOutput
{
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
    public ulong Index { get; init; }
    public ulong Amount { get; init; }
    public string Address { get; init; } = string.Empty;
    public IEnumerable<TxInput> Inputs { get; init; } = new List<TxInput>();
    public IEnumerable<Asset>? Assets { get; set; }
}