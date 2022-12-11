namespace Conclave.Common.Responses;

public class PoolDetails
{
    public string PoolId { get; init; } = string.Empty;
    public string Ticker { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Margin { get; init; }
    public int Blocks { get; init; }
    public decimal Saturation { get; init; }
    public float Apy { get; init; }
}