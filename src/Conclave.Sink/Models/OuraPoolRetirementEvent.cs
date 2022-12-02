using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraPoolRetirementEvent : OuraEvent
{
    [JsonPropertyName("pool_retirement")]
    public OuraPoolRetirement? PoolRetirement { get; init; }
}