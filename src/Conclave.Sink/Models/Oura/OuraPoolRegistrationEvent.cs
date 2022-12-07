using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraPoolRegistrationEvent : OuraEvent
{
    [JsonPropertyName("pool_registration")]
    public OuraPoolRegistration? PoolRegistration { get; init; }
}