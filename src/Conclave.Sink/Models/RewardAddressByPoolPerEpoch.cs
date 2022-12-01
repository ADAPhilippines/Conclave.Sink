
namespace Conclave.Sink.Models;

public class RewardAddressByPoolPerEpoch
{
    public string PoolId { get; set; } = string.Empty;
    public string RewardAddress { get; set; } = string.Empty;
    public ulong Slot { get; set; }
    public Block? Block { get; set; }
}
