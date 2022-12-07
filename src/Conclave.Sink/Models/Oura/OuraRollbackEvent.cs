using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraRollbackEvent : OuraEvent
{
    [JsonPropertyName("roll_back")]
    public OuraRollback? RollBack { get; init; }
}