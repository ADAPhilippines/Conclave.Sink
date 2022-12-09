using System.Text.Json;

namespace Conclave.Common.Models.Entities;

public class PoolRegistration
{
    public string PoolId { get; init; } = string.Empty;
    public string VrfKeyHash { get; init; } = string.Empty;
    public ulong Pledge { get; init; }
    public ulong Cost { get; init; }
    public decimal Margin { get; init; }
    public string RewardAccount { get; init; } = string.Empty;
    public List<string> PoolOwners { get; init; } = new();
    public List<string> Relays { get; init; } = new();
    public JsonDocument? PoolMetadataJSON { get; init; }
    public string? PoolMetadataString { get; init; } = string.Empty;
    public string? PoolMetadataHash { get; init; }
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
}