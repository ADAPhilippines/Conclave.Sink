namespace Conclave.Common.Models;

public record ConclavePool
{
    public string PoolId { get; init; } = string.Empty;
    public ulong Since { get; init; }
    public ulong Until { get; init; }
}