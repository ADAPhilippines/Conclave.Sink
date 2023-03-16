using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public class Metadatum
{
    public string? Label { get; init; }

    [JsonPropertyName("map_json")]
    public JsonElement Content { get; init; }
}