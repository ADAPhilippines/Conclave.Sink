using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public class OuraStakeDelegation
{
    [JsonPropertyName("pool_hash")]
    public string PoolHash { get; set; } = string.Empty;
    public Credential? Credential { get; set; }
}