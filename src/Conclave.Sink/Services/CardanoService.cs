using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Extensions;
using Conclave.Sink.Models;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Services;

public class CardanoService
{
    private readonly ConclaveSinkSettings _settings;
    public CardanoService(IOptions<ConclaveSinkSettings> settings)
    {
        _settings = settings.Value;
    }

    public ulong CalculateEpochBySlot(ulong slot) => slot / _settings.EpochLength;

    public string PoolHashToBech32(string poolId) => Bech32.Encode(poolId.HexToByteArray(), "pool");
}