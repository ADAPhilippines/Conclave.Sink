using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public class MintAsset
{
    public string? Policy { get; init; }
    public string? Asset { get; init; }
}