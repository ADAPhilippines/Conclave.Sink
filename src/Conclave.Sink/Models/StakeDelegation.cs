
namespace Conclave.Sink.Models;

public class StakeDelegation
{
    public string StakeAddress { get; init; } = string.Empty;
    public string PoolId { get; init; } = string.Empty;
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
}
