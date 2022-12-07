using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraTransactionEvent : OuraEvent
{
    public OuraTransaction? Transaction { get; init; }
}
