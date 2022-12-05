using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraTransactionEvent : OuraEvent
{
    public TransactionData? Transaction { get; init; }
}
