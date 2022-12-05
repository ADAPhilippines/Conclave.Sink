
namespace Conclave.Sink.Models;

public class DelegatorByPoolEpoch
{
    public string StakeAddress { get; init; } = string.Empty;
    public string PoolId { get; init; } = string.Empty;
    public string TxHash { get; init; } = string.Empty;
    public ulong TxIndex { get; init; }
    public Block Block { get; set; } = new();
}
