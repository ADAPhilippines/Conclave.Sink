
namespace Conclave.Sink.Models;

public class BalanceByStakeAddressEpoch
{
    public string StakeAddress { get; set; } = string.Empty;
    public ulong? Epoch { get; set; }
    public ulong Balance { get; set; }
}