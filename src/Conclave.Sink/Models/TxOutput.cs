namespace Conclave.Sink.Models;

public class TxOutput
{
    public string TxHash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public ulong Amount { get; init; }
    public string Address { get; init; } = string.Empty;
    public Block Block { get; init; } = new Block();
    public IEnumerable<TxInput> Inputs { get; init; } = new List<TxInput>();
}