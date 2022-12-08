namespace Conclave.Common.Responses;

public class PoolDetails
{
    public string Name { get; init; } = string.Empty;
    public string Ticker { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ulong Blocks { get; init; }
    public float Saturation { get; init; }
    public ulong Pledge { get; init; }
    public float Apy { get; init; }
    public decimal Margin { get; init; }
}