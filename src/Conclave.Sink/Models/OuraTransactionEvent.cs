using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraTransactionEvent : OuraEvent
{
    public Transaction? Transaction { get; init; }
}
