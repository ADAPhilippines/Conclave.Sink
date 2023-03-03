namespace TeddySwap.Common.Models.Response;

public class PaginatedLeaderboardResponse : PaginatedResponse<LeaderboardResponse>
{
    public int TotalAmount { get; init; }
}