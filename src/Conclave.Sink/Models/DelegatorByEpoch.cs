
namespace Conclave.Sink.Models;

public class DelegatorByEpoch
{
    public string StakeAddress { get; set; } = string.Empty;
    public string PoolHash { get; set; } = string.Empty;
    public ulong Slot { get; set; }
    public Block? Block { get; set; }
}

public record DelegatorByPool(string StakeAddress, ulong SinceEpoch);