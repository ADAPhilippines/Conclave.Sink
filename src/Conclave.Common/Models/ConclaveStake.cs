namespace Conclave.Common.Models;

public record ConclaveStake
{
    public ulong Lovelace { get; set; }
    public ulong Conclave { get; set; }
}