using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public class OuraTxInput
{
    [JsonPropertyName("tx_id")]
    public string TxHash { get; init; } = string.Empty;
    public ulong Index { get; init; }
}