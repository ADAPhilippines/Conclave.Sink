using System.Text.Json;

namespace TeddySwap.Common.Models.Response;

public class AssetMetadataResponse
{
    public AssetClass AssetClass { get; init; } = new();
    public string? Metadata { get; init; } = string.Empty;
}