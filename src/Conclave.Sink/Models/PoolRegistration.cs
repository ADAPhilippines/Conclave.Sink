
using System.Text.Json;

namespace Conclave.Sink.Models;

public class PoolRegistration
{
    public string PoolId { get; init; } = string.Empty;
    public string PoolIdBech32 { get; init; } = string.Empty;
    public string VRFKeyHash { get; init; } = string.Empty;
    public ulong Pledge { get; init; }
    public ulong Cost { get; init; }
    public decimal Margin { get; init; }
    public string RewardAccount { get; init; } = string.Empty;
    public List<string> PoolOwners { get; init; } = new();
    public List<string> Relays { get; init; } = new();
    public JsonDocument? PoolMetadataJSON { get; init; }
    public string? PoolMetadataString { get; init; } = string.Empty;
    public string? PoolMetadataHash { get; init; }
    public Block? Block { get; init; } = new();
    public string? BlockHash { get; init; } = string.Empty;
    public string? TxHash { get; init; } = string.Empty;
    public Transaction? Transaction { get; init; } = new();
}