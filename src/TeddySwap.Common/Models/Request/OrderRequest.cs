namespace TeddySwap.Common.Models.Request;

public class OrderRequest : RequestBase
{
    public OrderType? orderType { get; init; } = OrderType.All;
    public string? Address { get; init; }
}