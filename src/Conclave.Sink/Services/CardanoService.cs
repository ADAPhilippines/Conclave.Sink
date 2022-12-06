using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Conclave.Sink.Models;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Services;

public class CardanoService
{
    private const int MAX_BYTE_LENGTH = 28;
    private readonly ConclaveSinkSettings _settings;
    public CardanoService(IOptions<ConclaveSinkSettings> settings)
    {
        _settings = settings.Value;
    }

    public ulong CalculateEpochBySlot(ulong slot) => slot / _settings.EpochLength;

    public string PoolHashToBech32(string poolId) => Bech32.Encode(poolId.HexToByteArray(), "pool");

    public string RewardAddressHashToBech32(string stakeAddress)
    {
        byte[] stakeAddressbyteArray = stakeAddress.HexToByteArray();
        if (stakeAddressbyteArray.Count() > MAX_BYTE_LENGTH)
        {
            stakeAddressbyteArray = stakeAddressbyteArray.Skip(1).ToArray();
        }

        return AddressUtility.GetRewardAddress(stakeAddressbyteArray, NetworkType.Preview).ToString();
    }
}