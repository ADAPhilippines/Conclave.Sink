namespace TeddySwap.Common.Models.Response;

public class PaginatedResponse<T>
{
    public List<T> Result { get; init; } = new();
    public int TotalCount { get; init; }
}