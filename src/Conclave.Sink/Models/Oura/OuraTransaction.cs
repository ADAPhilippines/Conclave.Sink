using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public class OuraTransaction
{
    public ulong? Fee { get; init; }
    public IEnumerable<OuraWithdrawal>? Withdrawals { get; init; }
    
    [JsonPropertyName("collateral_inputs")]
    public IEnumerable<OuraTxInput>? CollateralInputs { get; init;}
    
    [JsonPropertyName("collateral_output")]
    public OuraTxOutput? CollateralOutput { get; init;}
}