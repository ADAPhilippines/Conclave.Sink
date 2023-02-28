using Microsoft.Extensions.Options;
using PeterO.Cbor2;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Models;
using System.Numerics;

namespace TeddySwap.Sink.Services;

public class DatumService
{
    private readonly CborService _cborService;
    private readonly TeddySwapSinkSettings _settings;
    public DatumService(CborService cborService, IOptions<TeddySwapSinkSettings> settings)
    {
        _cborService = cborService;
        _settings = settings.Value;
    }

    public PoolDatum CborToPoolDatum(CBORObject cbor)
    {
        if (cbor.Count != 5)
        {
            throw new InvalidDataException("Not a valid pool cbor");
        }

        PoolDatum poolDatum = new();
        poolDatum.Nft = CborToAssetClass(cbor[0]);
        poolDatum.ReserveX = CborToAssetClass(cbor[1]);
        poolDatum.ReserveY = CborToAssetClass(cbor[2]);
        poolDatum.Lq = CborToAssetClass(cbor[3]);
        poolDatum.Fee = (uint)_cborService.DecodeValueByCborType(cbor[4]);

        return poolDatum;
    }

    public DepositDatum CborToDepositDatum(CBORObject cbor)
    {
        if (cbor.Count != 8)
        {
            throw new InvalidDataException("Not a valid pool cbor");
        }

        DepositDatum depositDatum = new();
        depositDatum.Nft = CborToAssetClass(cbor[0]);
        depositDatum.ReserveX = CborToAssetClass(cbor[1]);
        depositDatum.ReserveY = CborToAssetClass(cbor[2]);
        depositDatum.Lq = CborToAssetClass(cbor[3]);
        depositDatum.ExFee = (uint)_cborService.DecodeValueByCborType(cbor[4]);
        depositDatum.RewardPkh = ((string)_cborService.DecodeValueByCborType(cbor[5])).ToLower();
        depositDatum.StakePkh = ((string)_cborService.DecodeValueByCborType(cbor[6][0])).ToLower();
        depositDatum.CollateralAda = (uint)_cborService.DecodeValueByCborType(cbor[7]);

        return depositDatum;
    }

    public RedeemDatum CborToRedeemDatum(CBORObject cbor)
    {
        if (cbor.Count != 7)
        {
            throw new InvalidDataException("Not a valid pool cbor");
        }

        RedeemDatum redeemDatum = new();
        redeemDatum.Nft = CborToAssetClass(cbor[0]);
        redeemDatum.ReserveX = CborToAssetClass(cbor[1]);
        redeemDatum.ReserveY = CborToAssetClass(cbor[2]);
        redeemDatum.Lq = CborToAssetClass(cbor[3]);
        redeemDatum.ExFee = (uint)_cborService.DecodeValueByCborType(cbor[4]);
        redeemDatum.RewardPkh = ((string)_cborService.DecodeValueByCborType(cbor[5])).ToLower();
        redeemDatum.StakePkh = ((string)_cborService.DecodeValueByCborType(cbor[6][0])).ToLower();

        return redeemDatum;
    }

    public SwapDatum CborToSwapDatum(CBORObject cbor)
    {
        if (cbor.Count != 9)
        {
            throw new InvalidDataException("Not a valid pool cbor");
        }

        SwapDatum swapDatum = new();
        swapDatum.Base = CborToAssetClass(cbor[0]);
        swapDatum.Quote = CborToAssetClass(cbor[1]);
        swapDatum.Nft = CborToAssetClass(cbor[2]);
        swapDatum.Fee = (uint)_cborService.DecodeValueByCborType(cbor[3]);
        swapDatum.ExFeePerTokenNum = BigInteger.Parse(_cborService.DecodeValueByCborType(cbor[4]).ToString() ?? "0");
        swapDatum.ExFeePerTokenDen = BigInteger.Parse(_cborService.DecodeValueByCborType(cbor[5]).ToString() ?? "0");
        swapDatum.RewardPkh = ((string)_cborService.DecodeValueByCborType(cbor[6])).ToLower();
        swapDatum.StakePkh = ((string)_cborService.DecodeValueByCborType(cbor[7][0])).ToLower();
        swapDatum.BaseAmount = BigInteger.Parse(_cborService.DecodeValueByCborType(cbor[8]).ToString() ?? "0");
        swapDatum.MinQuoteAmount = BigInteger.Parse(_cborService.DecodeValueByCborType(cbor[9]).ToString() ?? "0");

        return swapDatum;
    }

    public AssetClass CborToAssetClass(CBORObject cbor)
    {
        var assetClass = new AssetClass();
        assetClass.PolicyId = ((string)_cborService.DecodeValueByCborType(cbor[0])).ToLower();
        assetClass.Name = ((string)_cborService.DecodeValueByCborType(cbor[1])).ToLower();

        return assetClass;
    }

    public OrderType GetOrderType(string validatorAddr)
    {
        if (validatorAddr == _settings.DepositAddress)
        {
            return OrderType.Deposit;
        }
        else if (validatorAddr == _settings.RedeemAddress)
        {
            return OrderType.Redeem;
        }
        else if (validatorAddr == _settings.SwapAddress)
        {
            return OrderType.Swap;
        }
        else
        {
            return OrderType.Unknown;
        }
    }

    public bool IsValidAsset(CBORObject cbor)
    {
        return (cbor.ContainsKey(0) && cbor[0].Type == CBORType.ByteString) && (cbor.ContainsKey(1) && cbor[1].Type == CBORType.ByteString) && !cbor.ContainsKey(2);
    }
}