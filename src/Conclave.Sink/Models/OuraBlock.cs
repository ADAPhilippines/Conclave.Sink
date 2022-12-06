using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class OuraBlock
{
    [JsonPropertyName("vrf_vkey")]
    public string? VrfVkey { get; set; }
    public string? Era { get; set; }
}