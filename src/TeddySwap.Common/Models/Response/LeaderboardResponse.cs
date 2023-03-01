namespace TeddySwap.Common.Models.Response;

public class LeaderboardResponse
{
    string Address { get; init; } = string.Empty;
    int Rank { get; set; }
    int Total { get; init; }
    int Deposit { get; init; }
    int Redeem { get; init; }
    int Swap { get; init; }
    int Batch { get; init; }
}