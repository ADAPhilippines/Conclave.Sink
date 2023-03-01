namespace TeddySwap.Common.Models.Response;

public class LeaderboardHistoryResponse : ResponseBase
{
    public List<LeaderboardResponse> LeaderboardHistory { get; init; } = new();
}