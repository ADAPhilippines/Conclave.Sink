using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class BlockData
{

    [JsonPropertyName("vrf_vkey")]
    public string VrfVkey { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;
}