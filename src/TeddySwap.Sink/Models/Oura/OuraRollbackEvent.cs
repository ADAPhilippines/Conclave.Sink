using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraRollbackEvent : OuraEvent
{
    [JsonPropertyName("roll_back")]
    public OuraRollback? RollBack { get; init; }
}