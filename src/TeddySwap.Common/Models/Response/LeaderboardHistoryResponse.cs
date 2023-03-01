namespace TeddySwap.Common.Models.Response;

public class LeaderbordHistoryResponse : ResponseBase
{
    List<LeaderboardResponse> LeaderboardHistory { get; init; } = new();
}