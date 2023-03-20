namespace TeddySwap.Common.Models.Response;

public class AssetResponse
{
    public string Name { get; init; } = string.Empty;
    public string AsciiName { get; init; } = string.Empty;
    public ulong Amount { get; init; }
}