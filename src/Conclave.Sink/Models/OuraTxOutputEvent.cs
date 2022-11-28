using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraTxOutputEvent : OuraEvent
{
    [JsonPropertyName("tx_output")]
    public OuraTxOutput? TxOutput { get; init; }
}