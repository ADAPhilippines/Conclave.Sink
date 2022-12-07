using CardanoSharp.Wallet.Enums;

namespace Conclave.Sink.Models;

public record ConclaveSinkSettings
{
    public ulong EpochLength { get; init; }
    public NetworkType NetworkType { get; init; }
    public string ConclaveTokenPolicy { get; set; } = string.Empty;
    public string ConclaveTokenAssetName { get; set; } = string.Empty;
    public IEnumerable<string> Reducers { get; set; } = new List<string>();
}