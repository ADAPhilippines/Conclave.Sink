namespace Conclave.Common.Models;

public record AccountEpochStake
{
    public string PoolId { get; init; } = string.Empty;
    public ulong Epoch { get; init; }
    public ulong Lovelace { get; init; }
    public ulong Conclave { get; init; }
}