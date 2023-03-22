using CardanoSharp.Wallet.Enums;
namespace TeddySwap.Sink.Models;

public record TeddySwapSinkSettings
{
    public ulong EpochLength { get; init; }
    public NetworkType NetworkType { get; init; }
    public IEnumerable<string> Reducers { get; set; } = new List<string>();
    public IEnumerable<string> NftPolicyIds { get; init; } = new List<string>();
    public IEnumerable<FisoPool> FisoPools { get; init; } = new List<FisoPool>();
    public ulong FisoStartEpoch { get; init; }
    public ulong FisoEndEpoch { get; init; }
    public decimal FisoRewardPerEpoch { get; init; }
    public string PoolAddress { get; init; } = string.Empty;
    public string DepositAddress { get; init; } = string.Empty;
    public string RedeemAddress { get; init; } = string.Empty;
    public string SwapAddress { get; init; } = string.Empty;
}