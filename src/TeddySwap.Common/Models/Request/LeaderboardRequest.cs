namespace TeddySwap.Common.Models.Request;

public class LeaderboardRequest : RequestBase
{
    public OrderType? orderType { get; init; } = OrderType.All;
    public string? Address { get; init; }
}