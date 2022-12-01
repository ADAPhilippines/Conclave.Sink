using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraStakeRegistrationEvent : OuraEvent
{
    [JsonPropertyName("stake_registration")]
    public StakeRegistration? StakeRegistration { get; set; }
}

public class StakeRegistration
{
    public Credential? Credential { get; set; }
}