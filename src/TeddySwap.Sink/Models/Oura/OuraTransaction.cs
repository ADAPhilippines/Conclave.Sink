using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public class OuraTransaction
{
    public ulong? Fee { get; init; }
    public IEnumerable<OuraWithdrawal>? Withdrawals { get; init; }
    public IEnumerable<OuraTxInput>? Inputs { get; init; }
    public IEnumerable<OuraTxOutput>? Outputs { get; init; }

    [JsonPropertyName("collateral_inputs")]
    public IEnumerable<OuraTxInput>? CollateralInputs { get; init; }

    [JsonPropertyName("collateral_output")]
    public OuraTxOutput? CollateralOutput { get; init; }
}