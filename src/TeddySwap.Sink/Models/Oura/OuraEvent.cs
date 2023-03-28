using System.Text.Json.Serialization;
using TeddySwap.Sink.Extensions;

namespace TeddySwap.Sink.Models.Oura;

public record OuraEvent : IOuraEvent
{
    public OuraContext? Context { get; set; }
    public string? Fingerprint { get; init; }

    [JsonConverter(typeof(OuraVariantJsonConverter))]
    public OuraVariant? Variant { get; init; }

    public ulong? Timestamp { get; init; }
}