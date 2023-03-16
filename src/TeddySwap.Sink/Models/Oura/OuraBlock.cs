using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public class OuraBlock
{

    [JsonPropertyName("vrf_vkey")]
    public string VrfVkey { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;

    [JsonPropertyName("invalid_transactions")]
    public IEnumerable<ulong>? InvalidTransactions { get; set; }
    public List<OuraTransaction>? Transactions { get; set; }
}