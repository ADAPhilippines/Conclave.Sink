using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraPoolRegistrationEvent : OuraEvent
{
    [JsonPropertyName("pool_registration")]
    public OuraPoolRegistration? PoolRegistration { get; init; }
}