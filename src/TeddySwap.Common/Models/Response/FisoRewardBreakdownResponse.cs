namespace TeddySwap.Common.Models.Response;

public class FisoRewardBreakdownResponse
{
    public string StakeAddress { get; init; } = string.Empty;
    public List<FisoRewardResponse> FisoRewards { get; init; } = new();
    public ulong TotalBaseReward { get; init; }
    public ulong TotalBonusReward { get; init; }
}