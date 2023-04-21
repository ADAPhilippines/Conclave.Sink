namespace TeddySwap.UI.Services;

public class RewardService
{
    private const double ROUND_ONE_MULTIPLIER = 2.75;
    private const int EFFECTIVE_NFTS = 4_246;
    private const int AVAILABLE_REWARDS = 33_453_000;
    private const string ROUND_ONE_POLICY_ID = "a";
    private const string ROUND_TWO_POLICY_ID = "s";

    public double CalculateNfTRewardAsync(string policyId, string nftName) 
    {
        return policyId switch
        {
            ROUND_ONE_POLICY_ID => CalculateRoundOneNftRewardAsync(nftName),
            ROUND_TWO_POLICY_ID => CalculateRoundTwoNftRewardAsync(nftName),
            _ => throw new ArgumentException("Invalid Policy ID")
        };
    }

    public double CalculateRoundOneNftRewardAsync(string nftName) 
    {
        int rank = 1; // @TODO: Replace with actual rank
        int baseReward = GetRoundOneBaseReward(rank);
        double roundBonusReward = baseReward * 0.10;
        double rtrPoolReward = AVAILABLE_REWARDS * (ROUND_ONE_MULTIPLIER / EFFECTIVE_NFTS * 100);
        return baseReward + roundBonusReward + rtrPoolReward;
    }

    public double CalculateRoundTwoNftRewardAsync(string nftName) 
    {
        int mintOrder = 1;
        int rank = 1;
        int baseReward = GetRoundTwoBaseReward(rank);
        double roundBonusReward = GetRoundTwoBonus(mintOrder);
        double rtrPoolReward = 1 / EFFECTIVE_NFTS * AVAILABLE_REWARDS;
        return baseReward + roundBonusReward +rtrPoolReward;
    }

    private int GetRoundOneBaseReward(int rank) 
    {
        return rank switch 
        {
            (< 16)    => 28_000,
            (< 101)   => 17_500,
            (< 251)   => 12_600,
            (< 501)   => 11_200,
            (> 500)   => 10_500
        }; 
    }

    private int GetRoundTwoBaseReward(int rank) 
    {
        return rank switch 
        {
            (< 24)    => 7_000,
            (< 222)   => 6_000,
            (< 665)   => 5_000,
            (< 1328)  => 4_500,
            (> 1327)  => 4_200
        }; 
    }

    private int GetRoundTwoBonus(int mintOrder)
    {
        return mintOrder switch
        {
            (<= 1_200) => 10_733,
            (<= 1_400) => 9_733,
            (<= 1_600) => 8_400,
            (<= 1_800) => 6_400,
            (>= 1_800) and (<= 2_000) => 8_400,
            (> 2_000) => 4_000
        };
    }
}
