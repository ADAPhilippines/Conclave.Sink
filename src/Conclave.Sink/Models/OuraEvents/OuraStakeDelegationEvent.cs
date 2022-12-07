using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.OuraEvents;

public record OuraStakeDelegationEvent : OuraEvent
{
    [JsonPropertyName("stake_delegation")]
    public OuraStakeDelegation? StakeDelegation { get; set; }
}