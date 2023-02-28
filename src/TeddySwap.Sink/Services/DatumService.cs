using PeterO.Cbor2;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Services;

public class DatumService
{
    private readonly CborService _cborService;
    public DatumService(CborService cborService)
    {
        _cborService = cborService;
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

    public AssetClass CborToAssetClass(CBORObject cbor)
    {
        var assetClass = new AssetClass();
        assetClass.PolicyId = ((string)_cborService.DecodeValueByCborType(cbor[0])).ToLower();
        assetClass.Name = ((string)_cborService.DecodeValueByCborType(cbor[1])).ToLower();

        return assetClass;
    }

    public bool IsValidAsset(CBORObject cbor)
    {
        return (cbor.ContainsKey(0) && cbor[0].Type == CBORType.ByteString) && (cbor.ContainsKey(1) && cbor[1].Type == CBORType.ByteString) && !cbor.ContainsKey(2);
    }
}