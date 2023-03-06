namespace TeddySwap.Common.Models.Request;

public class WalletRewardsRequest
{
    public List<string> Addresses { get; init; } = new();
}