using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTransactionEvent : OuraEvent
{
    public OuraTransaction? Transaction { get; init; }
}
