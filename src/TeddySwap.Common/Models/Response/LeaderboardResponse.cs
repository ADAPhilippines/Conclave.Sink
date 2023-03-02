namespace TeddySwap.Common.Models.Response;

public class LeaderboardResponse
{
    public string TestnetAddress { get; init; } = string.Empty;
    public string MainnetAddress { get; init; } = string.Empty;
    public int Rank { get; set; }
    public decimal BaseReward { get; set; }
    public decimal BaseRewardPercentage { get; set; }
    public int Total { get; init; }
    public int Deposit { get; init; }
    public int Redeem { get; init; }
    public int Swap { get; init; }
    public int Batch { get; init; }
}