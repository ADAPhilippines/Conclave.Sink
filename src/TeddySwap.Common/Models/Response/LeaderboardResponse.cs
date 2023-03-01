namespace TeddySwap.Common.Models.Response;

public class LeaderboardResponse
{
    public string Address { get; init; } = string.Empty;
    public int Rank { get; set; }
    public int Total { get; init; }
    public int Deposit { get; init; }
    public int Redeem { get; init; }
    public int Swap { get; init; }
    public int Batch { get; init; }
}