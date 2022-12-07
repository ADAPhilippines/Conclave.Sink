using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.OuraEvents;

public record OuraPoolRegistrationEvent : OuraEvent
{
    [JsonPropertyName("pool_registration")]
    public OuraPoolRegistration? PoolRegistration { get; init; }
}