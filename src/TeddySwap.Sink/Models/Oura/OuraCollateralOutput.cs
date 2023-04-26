using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraCollateralOutput : OuraEvent
{
    public string Address { get; init; } = string.Empty;
    public ulong Amount { get; init; }
}