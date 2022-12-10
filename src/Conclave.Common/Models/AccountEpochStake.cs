namespace Conclave.Common.Models;

public record AccountEpochStake
{
    public string PoolId { get; init; } = string.Empty;
    public ulong Epoch { get; init; }
    public ulong Lovelace { get; set; }
    public ulong Conclave { get; set; }
}