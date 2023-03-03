namespace TeddySwap.Common.Models.Response;

public class AssetResponse
{
    public string Address { get; init; } = string.Empty;
    public string PolicyId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ulong Amount { get; init; }
}