namespace TeddySwap.Common.Models.Response;

public class PaginatedLeaderBoardResponse : PaginatedResponse<LeaderBoardResponse>
{
    public int TotalAmount { get; init; }
}