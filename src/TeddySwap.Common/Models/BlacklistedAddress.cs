namespace TeddySwap.Common.Models;

public record BlacklistedAddress
{
    public string Address { get; init; } = string.Empty;
}