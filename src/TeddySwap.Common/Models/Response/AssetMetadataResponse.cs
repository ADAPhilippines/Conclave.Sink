using System.Text.Json;

namespace TeddySwap.Common.Models.Response;

public class AssetMetadataResponse
{
    public AssetResponse AssetResponse { get; init; } = new();
    public JsonElement Metadata { get; init; } = new();
}