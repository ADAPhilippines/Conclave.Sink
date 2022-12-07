using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.OuraEvents;

public record OuraTxInputEvent : OuraEvent
{
    [JsonPropertyName("tx_input")]
    public OuraTxInput? TxInput { get; init; }
}