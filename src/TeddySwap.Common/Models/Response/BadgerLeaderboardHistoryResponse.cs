namespace TeddySwap.Common.Models.Response;

public class BadgerLeaderbordHistoryResponse : ResponseBase
{
    public List<BadgerLeaderboardResponse> BadgerLeaderboardHistory { get; init; } = new();
}