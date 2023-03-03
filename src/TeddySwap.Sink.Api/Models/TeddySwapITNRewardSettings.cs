namespace TeddySwap.Sink.Api.Models;

public record TeddySwapITNRewardSettings
{
    public int TotalReward { get; init; }
    public int BatcherReward { get; init; }
    public int UserReward { get; init; }
}