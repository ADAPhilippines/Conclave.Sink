namespace TeddySwap.Common.Models.Response;

public class PaginatedLeaderboardResponse : PaginatedResponse<LeaderBoardResponse>
{
    public int TotalAmount { get; init; }
}