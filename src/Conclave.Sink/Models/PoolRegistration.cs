
using System.Text.Json;

namespace Conclave.Sink.Models;

public class PoolRegistration
{
    public string Operator { get; init; } = string.Empty;
    public string VRFKeyHash { get; init; } = string.Empty;
    public ulong Pledge { get; init; }
    public ulong Cost { get; init; }
    public decimal Margin { get; init; }
    public string RewardAccount { get; init; } = string.Empty;
    public List<string> PoolOwners { get; init; } = new();
    public List<string> Relays { get; init; } = new();
    public string? TxHash { get; init; } = string.Empty;
    public JsonDocument? PoolMetadata { get; init; }
    public string? PoolMetadataHash { get; init; }
    public Block? Block { get; init; } = new();
}