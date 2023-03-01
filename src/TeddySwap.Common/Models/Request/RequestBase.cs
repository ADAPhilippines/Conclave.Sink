namespace TeddySwap.Common.Models.Request;

public class RequestBase
{
    public int? Offset { get; init; } = 0;
    public int? Limit { get; init; } = 100;
}