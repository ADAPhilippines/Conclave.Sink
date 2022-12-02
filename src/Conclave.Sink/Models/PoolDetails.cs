
using System.Text.Json;

namespace Conclave.Sink.Models;

public enum PoolStatus
{
    Registered,
    Retired
}

public class PoolDetails
{
    public string Operator { get; set; } = string.Empty;
    public string VRFKeyHash { get; set; } = string.Empty;
    public ulong Pledge { get; set; }
    public ulong Cost { get; set; }
    public decimal Margin { get; set; }
    public string RewardAccount { get; set; } = string.Empty;
    public List<string> PoolOwners { get; set; } = new();
    public List<string> Relays { get; set; } = new();
    public string TxHash { get; set; } = string.Empty;
    public string PoolMetadata { get; init; } = string.Empty;
    public Block Block { get; set; } = new();
}