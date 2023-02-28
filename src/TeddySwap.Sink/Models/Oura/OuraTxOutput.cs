using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTxOutput
{
    public string? Address { get; init; }
    public ulong? Amount { get; init; }
    [JsonPropertyName("inline_datum")]
    public byte[]? InlineDatum { get; init; }
    public IEnumerable<OuraAsset>? Assets { get; set; }
}