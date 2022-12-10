namespace Conclave.Common.Models;

public record ConclaveSettings
{
    public IEnumerable<ConclavePool> Members { get; init; } = new List<ConclavePool>();
    public ulong Supply { get; init; }
    public ulong Duration { get; init; }
    public ulong DistributionStart { get; init; }
    public int DelegatorShare { get; init; }
    public int OperatorShare { get; init; }
    public int PartnerShare { get; init; }
}