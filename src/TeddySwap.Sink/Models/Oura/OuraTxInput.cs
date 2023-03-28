using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTxInput : OuraEvent
{
    [JsonPropertyName("tx_id")]
    public string TxHash { get; init; } = string.Empty;
    public ulong Index { get; init; }
}