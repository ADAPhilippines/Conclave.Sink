using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
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

    public string RewardAddressHashToBech32(string stakeAddress)
    {
        byte[] stakeAddressbyteArray = stakeAddress.HexToByteArray();
        if (stakeAddressbyteArray.Count() > 28)
        {
            stakeAddressbyteArray = stakeAddressbyteArray.Skip(1).ToArray();
        }

        return AddressUtility.GetRewardAddress(stakeAddressbyteArray, NetworkType.Preview).ToString();
   }
   
    public string? TryGetStakeAddress(string paymentAddress)
    {
        try
        {
            Address addr = new Address(paymentAddress);
            return addr.GetStakeAddress().ToString();
        }
        catch { }
        return null;
    }
}