namespace Conclave.Common.Models;

public record Withdrawal
{
    public string StakeAddress { get; init; } = string.Empty;
    public ulong Amount { get; init; }
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
}