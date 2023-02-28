using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTxOutputEvent : OuraEvent
{
    [JsonPropertyName("tx_output")]
    public OuraTxOutput? TxOutput { get; init; }
}