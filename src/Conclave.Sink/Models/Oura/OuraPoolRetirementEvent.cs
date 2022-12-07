using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraPoolRetirementEvent : OuraEvent
{
    [JsonPropertyName("pool_retirement")]
    public OuraPoolRetirement? PoolRetirement { get; init; }
}