namespace Conclave.Common.Models.Entities;

public class CnclvByStakeEpoch
{
    public string StakeAddress { get; set; } = string.Empty;
    public ulong Epoch { get; set; }
    public ulong Balance { get; set; }
}