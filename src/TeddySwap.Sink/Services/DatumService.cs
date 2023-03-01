using System.Numerics;
using Microsoft.Extensions.Options;
using PeterO.Cbor2;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Models;

namespace TeddySwap.Sink.Services;

public class DatumService
{
    private readonly CborService _cborService;
    private readonly TeddySwapSinkSettings _settings;
    private readonly ILogger<DatumService> _logger;
    public DatumService(
        CborService cborService,
        IOptions<TeddySwapSinkSettings> settings,
        ILogger<DatumService> logger)
    {
        _cborService = cborService;
        _settings = settings.Value;
        _logger = logger;
    }

    public PoolDatum? CborToPoolDatum(CBORObject cbor)
    {
        try
        {
            CBORObject plutusData = cbor["plutus_data"]["fields"];
            PoolDatum poolDatum = new();
            poolDatum.Nft = CborToAssetClass(plutusData[0]);
            poolDatum.ReserveX = CborToAssetClass(plutusData[1]);
            poolDatum.ReserveY = CborToAssetClass(plutusData[2]);
            poolDatum.Lq = CborToAssetClass(plutusData[3]);
            poolDatum.Fee = uint.Parse(plutusData[4]["int"].ToString());
            return poolDatum;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.ToString());
            return null;
        }
    }

    // public DepositDatum CborToDepositDatum(CBORObject cbor)
    // {
    //     try
    //     {
    //         CBORObject plutusData = cbor["plutus_data"]["fields"];
    //         DepositDatum depositDatum = new();
    //         depositDatum.Nft = CborToAssetClass(plutusData[0]);
    //         depositDatum.ReserveX = CborToAssetClass(plutusData[1]);
    //         depositDatum.ReserveY = CborToAssetClass(plutusData[2]);
    //         depositDatum.Lq = CborToAssetClass(plutusData[3]);
    //         depositDatum.ExFee = uint.Parse(plutusData[4].ToString());
    //         depositDatum.RewardPkh = plutusData[5]["fields"]["bytes"].ToString();
    //         depositDatum.StakePkh = ((string)_cborService.DecodeValueByCborType(cbor[6][0])).ToLower();
    //         depositDatum.CollateralAda = (uint)_cborService.DecodeValueByCborType(cbor[7]);

    //         return depositDatum;
    //     }



    // }

    // public RedeemDatum CborToRedeemDatum(CBORObject cbor)
    // {
    //     cbor = cbor[1];
    //     if (cbor.Count != 7)
    //     {
    //         throw new InvalidDataException("Not a valid redeem cbor");
    //     }

    //     RedeemDatum redeemDatum = new();
    //     redeemDatum.Nft = CborToAssetClass(cbor[0]);
    //     redeemDatum.ReserveX = CborToAssetClass(cbor[1]);
    //     redeemDatum.ReserveY = CborToAssetClass(cbor[2]);
    //     redeemDatum.Lq = CborToAssetClass(cbor[3]);
    //     redeemDatum.ExFee = (uint)_cborService.DecodeValueByCborType(cbor[4]);
    //     redeemDatum.RewardPkh = ((string)_cborService.DecodeValueByCborType(cbor[5])).ToLower();
    //     redeemDatum.StakePkh = ((string)_cborService.DecodeValueByCborType(cbor[6][0])).ToLower();

    //     return redeemDatum;
    // }

    public SwapDatum? CborToSwapDatum(CBORObject cbor)
    {

        try
        {
            CBORObject plutusData = cbor["plutus_data"]["fields"];
            SwapDatum swapDatum = new();
            swapDatum.Base = CborToAssetClass(plutusData[0]);
            swapDatum.Quote = CborToAssetClass(plutusData[1]);
            swapDatum.Nft = CborToAssetClass(plutusData[2]);
            swapDatum.Fee = uint.Parse(plutusData[3]["int"].ToString());
            swapDatum.ExFeePerTokenNum = BigInteger.Parse(plutusData[4]["int"].ToString());
            swapDatum.ExFeePerTokenDen = BigInteger.Parse(plutusData[5]["int"].ToString());
            swapDatum.RewardPkh = "";
            swapDatum.StakePkh = "";
            swapDatum.BaseAmount = BigInteger.Parse(plutusData[8]["int"].ToString());
            swapDatum.MinQuoteAmount = BigInteger.Parse(plutusData[9]["int"].ToString());
            return swapDatum;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.ToString());
            return null;
        }


    }

    public AssetClass CborToAssetClass(CBORObject cbor)
    {
        var assetClass = new AssetClass();
        assetClass.PolicyId = cbor["fields"][0]["bytes"].ToString().Replace("\"", "");
        assetClass.Name = cbor["fields"][1]["bytes"].ToString().Replace("\"", "");

        assetClass.PolicyId = String.IsNullOrEmpty(assetClass.PolicyId) ? "lovelace" : assetClass.PolicyId;
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