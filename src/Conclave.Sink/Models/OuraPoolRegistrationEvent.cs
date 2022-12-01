using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraPoolRegistrationEvent : OuraEvent
{
    [JsonPropertyName("pool_registration")]
    public PoolRegistration? PoolRegistration { get; set; }
}

public class PoolRegistration
{
    public string Operator { get; set; } = string.Empty;
    [JsonPropertyName("vrf_keyhash")]
    public string VrfKeyhash { get; set; } = string.Empty;
    public ulong Pledge { get; set; }
    public ulong Cost { get; set; }
    public decimal Margin { get; set; }
    [JsonPropertyName("reward_account")]
    public string RewardAccount { get; set; } = string.Empty;
    [JsonPropertyName("pool_owners")]
    public List<string> PoolOwners { get; set; } = new List<string>();
    public List<string> Relays { get; set; } = new List<string>();
    // [JsonPropertyName("pool_metadata")]
    // public PoolMetadata? PoolMetadata { get; set; }
}

public class PoolMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string HomePage { get; set; } = string.Empty;
}