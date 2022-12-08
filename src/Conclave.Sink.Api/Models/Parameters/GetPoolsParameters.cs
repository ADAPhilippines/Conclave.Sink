using Microsoft.AspNetCore.Mvc;

namespace Conclave.Sink.Api.Parameters;

public enum PoolOrderParameters
{
    Margin,
    PoolId
}

//@TODO pure record
public record GetPoolsParameters
{
    public string? StakeAddress { get; init; }

    public bool IsConclave { get; init; } = false;

    public string? Filter { get; init; }

    public PoolOrderParameters? OrderBy { get; init; }

    public bool IsAscending { get; init; } = false;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 100;
}