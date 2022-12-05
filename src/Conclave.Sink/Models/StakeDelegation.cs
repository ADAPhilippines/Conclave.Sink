using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class StakeDelegation
{
    [JsonPropertyName("pool_hash")]
    public string PoolHash { get; set; } = string.Empty;
    public Credential? Credential { get; set; }
}