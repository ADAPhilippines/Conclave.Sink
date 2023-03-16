using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraTransaction : OuraEvent
{
    public string? Hash { get; init; }
    public int Index { get; set; }
    public ulong? Fee { get; init; }
    [JsonPropertyName("has_collateral_output")]
    public bool HasCollateralOutput { get; init; }
    public IEnumerable<OuraWithdrawal>? Withdrawals { get; init; }
    public IEnumerable<OuraTxInput>? Inputs { get; init; }
    public IEnumerable<OuraTxOutput>? Outputs { get; init; }
    [JsonPropertyName("collateral_inputs")]
    public IEnumerable<OuraTxInput>? CollateralInputs { get; init; }
    [JsonPropertyName("collateral_output")]
    public OuraTxOutput? CollateralOutput { get; init; }
    public IEnumerable<Metadatum>? Metadata { get; init; }
}