using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class TxInput
{
    public string TxHash { get; init; } = string.Empty;
    public string TxOutputHash {get; init; } = string.Empty;
    public ulong TxOutputIndex {get; init; }
    public TxOutput TxOutput { get; init; } = new TxOutput();
    public Block Block { get; init; } = new Block();
}