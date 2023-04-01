namespace TeddySwap.Common.Models.Response;

public class FisoRewardResponse
{
    public string PoolId { get; init; } = string.Empty;
    public ulong Epoch { get; init; }
    public ulong ActiveStake { get; init; }
    public ulong BaseReward { get; init; }
    public ulong BonusReward { get; set; }
}