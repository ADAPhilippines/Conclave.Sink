using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class PoolMetadata
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("ticker")]
    public string? Ticker { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("homepage")]
    public string? HomePage { get; set; }
}