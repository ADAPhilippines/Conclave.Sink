using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTxInputEvent : OuraEvent
{
    [JsonPropertyName("tx_input")]
    public OuraTxInput? TxInput { get; init; }
}