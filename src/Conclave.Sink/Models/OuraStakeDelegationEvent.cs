using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraStakeDelegationEvent : OuraEvent
{
    [JsonPropertyName("stake_delegation")]
    public StakeDelegation? StakeDelegation { get; set; }
}

public class StakeDelegation
{
    [JsonPropertyName("pool_hash")]
    public string PoolHash { get; set; } = string.Empty;
    public Credential? Credential { get; set; }
}

public class Credential
{
    public string AddrKeyHash { get; set; } = string.Empty;
    public string Scripthash { get; set; } = string.Empty;
}