namespace Conclave.Sink.Models;

public class TxOutput
{
    public string TxHash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public ulong Amount { get; set; }
    public string Address { get; set; } = string.Empty;
    public Block? Block { get; set; }
}