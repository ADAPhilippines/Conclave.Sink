namespace Conclave.Common.Models;

public record ConclaveStake
{
    public ulong Lovelace { get; init; }
    public ulong Conclave { get; init; }
}