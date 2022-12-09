namespace Conclave.Common.Models.Entities;

public class BalanceByAddress
{
    public string Address { get; set; } = string.Empty;
    public ulong Balance { get; set; }
}