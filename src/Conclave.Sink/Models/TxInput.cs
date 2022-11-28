using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class TxInput
{
    public string TxHash { get; init; } = string.Empty;
    public ulong Index { get; init; }
}