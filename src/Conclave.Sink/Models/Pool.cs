
namespace Conclave.Sink.Models;

public class Pool
{
    public string Operator { get; set; } = string.Empty;
    public string VRFKeyHash { get; set; } = string.Empty;
    public ulong Pledge { get; set; }
    public ulong Cost { get; set; }
    public decimal Margin { get; set; }
    public string RewardAccount { get; set; } = string.Empty;
    public List<string> PoolOwners { get; set; } = new();
    public List<string> Relays { get; set; } = new();
    public string PoolMetadata { get; set; } = string.Empty;
}