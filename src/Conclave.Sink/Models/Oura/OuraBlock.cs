using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public class OuraBlock
{

    [JsonPropertyName("vrf_vkey")]
    public string VrfVkey { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;

    [JsonPropertyName("invalid_transactions")]
    public IEnumerable<int>? InvalidTransactions { get; set; }
}