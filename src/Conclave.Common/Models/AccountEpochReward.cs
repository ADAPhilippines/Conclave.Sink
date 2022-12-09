namespace Conclave.Common.Models;

public record AccountEpochReward
{
    public ulong Epoch { get; init; }
    public ulong Amount { get; init; }
}