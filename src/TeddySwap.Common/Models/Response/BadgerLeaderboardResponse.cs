namespace TeddySwap.Common.Models.Response;

public class BadgerLeaderboardResponse
{
    string Address { get; init; } = string.Empty;
    int Rank { get; set; }
    int Total { get; init; }
}