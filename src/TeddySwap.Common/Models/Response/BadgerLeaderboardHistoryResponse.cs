namespace TeddySwap.Common.Models.Response;

public class BadgerLeaderbordHistoryResponse : ResponseBase
{
    List<BadgerLeaderboardResponse> BadgerLeaderboardHistory { get; init; } = new();
}