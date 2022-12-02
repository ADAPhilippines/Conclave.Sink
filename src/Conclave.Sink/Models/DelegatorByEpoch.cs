
namespace Conclave.Sink.Models;

public class DelegatorByEpoch
{
    public string StakeAddress { get; set; } = string.Empty;
    public string PoolId { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
    public ulong TxIndex { get; set; }
    public Block? Block { get; set; }
}

public record DelegatorByPool(string StakeAddress, ulong SinceEpoch);