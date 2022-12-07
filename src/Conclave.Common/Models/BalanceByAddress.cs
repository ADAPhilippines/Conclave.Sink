namespace Conclave.Common.Models;

public class BalanceByAddress
{
    public string Address { get; set; } = string.Empty;
    public ulong Balance { get; set; }
}