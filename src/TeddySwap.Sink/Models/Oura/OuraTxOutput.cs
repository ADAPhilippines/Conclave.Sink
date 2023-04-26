using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTxOutput : OuraEvent
{
    public string? Address { get; init; }
    public string? TxHash { get; set; }
    public ulong? TxIndex { get; set; }
    public ulong? OutputIndex { get; set; }
    public ulong? Amount { get; init; }
    [JsonPropertyName("datum_cbor")]
    public string? DatumCbor { get; init; }
    public IEnumerable<OuraAsset>? Assets { get; set; }
}