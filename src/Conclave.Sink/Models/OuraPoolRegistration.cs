using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraPoolRegistration : OuraEvent
{
    public string Operator { get; init; } = string.Empty;

    [JsonPropertyName("vrf_keyhash")]

    public string VRFKeyHash { get; init; } = string.Empty;
    
    public ulong Pledge { get; init; }

    public ulong Cost { get; init; }

    public decimal Margin { get; init; }

    [JsonPropertyName("reward_account")]
    public string RewardAccount { get; init; } = string.Empty;

    [JsonPropertyName("pool_owners")]
    public List<string> PoolOwners { get; init; } = new();
    
    public List<string> Relays { get; init; } = new();

    [JsonPropertyName("pool_metadata")]
    public string? PoolMetadata { get; init; }

    [JsonPropertyName("pool_metadata_hash")]
    public string? PoolMetadataHash { get; init; }
}