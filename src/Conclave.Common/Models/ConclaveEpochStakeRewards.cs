namespace Conclave.Common.Models;

public class ConclaveEpochStakeRewards
{
    public string StakeAddress { get; init; } = string.Empty;
    public ulong Epoch { get; init; }
    public ulong Lovelace { get; set; }
    public ulong Conclave { get; set; }
}