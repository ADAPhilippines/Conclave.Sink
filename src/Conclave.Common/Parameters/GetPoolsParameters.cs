namespace Conclave.Common.Parameters;

public record GetPoolsParameters
(
    string? StakeAddress,
    bool IsConclave,
    string? Filter,
    int Offset,
    int Limit
);