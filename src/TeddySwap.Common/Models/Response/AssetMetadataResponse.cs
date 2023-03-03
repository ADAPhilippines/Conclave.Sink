using System.Text.Json;

namespace TeddySwap.Common.Models.Response;

public class AssetMetadataResponse
{
    public AssetClass AssetClass { get; init; } = new();
    public JsonElement Metadata { get; init; } = new();
}