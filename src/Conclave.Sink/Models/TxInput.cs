using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class TxInput
{
    public string TxHash { get; init; } = string.Empty;
    public string TxInputOutputHash { get; init; } = string.Empty;
    public ulong TxInputOutputIndex { get; init; }
    public ulong Slot { get; set; }
    public Block? Block { get; set; }
}