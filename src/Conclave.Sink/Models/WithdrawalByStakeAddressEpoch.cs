
namespace Conclave.Sink.Models;

public class WithdrawalByStakeAddressEpoch
{
    public string StakeAddress { get; set; } = string.Empty;
    public ulong Amount { get; init; }
    public string Transactionhash { get; init; } = string.Empty;
    public Block Block { get; init; } = new();
}