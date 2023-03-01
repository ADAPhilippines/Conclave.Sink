namespace TeddySwap.Common.Models.Response;

public class BadgerLeaderboardResponse
{
    public string Address { get; init; } = string.Empty;
    public int Rank { get; set; }
    public int Total { get; init; }
}