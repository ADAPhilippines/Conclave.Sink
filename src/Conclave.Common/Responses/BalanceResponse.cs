
namespace Conclave.Common.Responses;

public record BalanceResponse
{
    public ulong Epoch { get; init; }
    public ulong Lovelace { get; init; }
    public ulong Conclave { get; init; }
}