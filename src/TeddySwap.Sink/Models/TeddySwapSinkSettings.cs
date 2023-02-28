using CardanoSharp.Wallet.Enums;

namespace TeddySwap.Sink.Models;

public record TeddySwapSinkSettings
{
    public ulong EpochLength { get; init; }
    public NetworkType NetworkType { get; init; }
    public IEnumerable<string> Reducers { get; set; } = new List<string>();
    public string PoolAddress { get; init; }
    public string DepositAddress { get; init; }
    public string RedeemAddress { get; init; }
    public string SwapAddress { get; init; }
}