using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraStakeDelegationEvent : OuraEvent
{
    [JsonPropertyName("stake_delegation")]
    public OuraStakeDelegation? StakeDelegation { get; set; }
}