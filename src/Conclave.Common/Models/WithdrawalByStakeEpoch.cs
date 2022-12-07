namespace Conclave.Common.Models;

public class WithdrawalByStakeEpoch
{
    public string? StakeAddress { get; init; }
    public ulong Epoch { get; init; }
    public ulong Amount { get; set; }
}