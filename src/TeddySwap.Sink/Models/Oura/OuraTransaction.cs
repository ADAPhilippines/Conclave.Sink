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
    public IEnumerable<OuraTxInput>? Inputs { get; set; }
    public IEnumerable<OuraTxOutput>? Outputs { get; set; }
    [JsonPropertyName("collateral_inputs")]
    public IEnumerable<OuraTxInput>? CollateralInputs { get; set; }
    [JsonPropertyName("collateral_output")]
    public OuraCollateralOutput? CollateralOutput { get; set; }
    public IEnumerable<Metadatum>? Metadata { get; init; }
    public IEnumerable<MintAsset>? Mint { get; init; }
}