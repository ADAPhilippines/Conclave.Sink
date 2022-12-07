using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraTxOutputEvent : OuraEvent
{
    [JsonPropertyName("tx_output")]
    public OuraTxOutput? TxOutput { get; init; }
}