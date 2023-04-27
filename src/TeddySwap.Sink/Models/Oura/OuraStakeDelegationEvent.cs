using System.Text.Json.Serialization;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Models;

public record OuraStakeDelegationEvent : OuraEvent
{
    [JsonPropertyName("stake_delegation")]
    public OuraStakeDelegation? StakeDelegation { get; set; }
}