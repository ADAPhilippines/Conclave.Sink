using System.Numerics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PeterO.Cbor2;
using TeddySwap.Common.Enums;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Services;

public class OrderService
{

    private readonly TeddySwapSinkSettings _settings;
    private readonly DatumService _datumService;
    private readonly ByteArrayService _byteArrayService;
    private readonly ILogger<OrderService> _logger;
    private IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;

    public OrderService(
        DatumService datumService,
        IOptions<TeddySwapSinkSettings> settings,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        ByteArrayService byteArrayService,
        ILogger<OrderService> logger)
    {
        _settings = settings.Value;
        _datumService = datumService;
        _byteArrayService = byteArrayService;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Order?> ProcessOrderAsync(OuraTransactionEvent transactionEvent)
    {
        TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        Order? order = null;

        if (transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Inputs is not null &&
            transactionEvent.Transaction.Outputs is not null)
        {
            List<string> inputRefs = transactionEvent.Transaction.Inputs.Select(i => i.TxHash + i.Index).ToList();
            List<TxOutput>? inputs = await _dbContext.TxOutputs
                .Include(o => o.Assets)
                .Where(o => inputRefs.Contains(o.TxHash + o.Index)).ToListAsync();

            List<string> validators = new()
                {
                    _settings.DepositAddress,
                    _settings.SwapAddress,
                    _settings.RedeemAddress
                };

            if (transactionEvent.Context is null) return null;

            // Find Validator Utxos
            TxOutput? poolInput = inputs.Where(i => i.Address == _settings.PoolAddress).FirstOrDefault();
            TxOutput? orderInput = inputs.Where(i => validators.Contains(i.Address)).FirstOrDefault();

            // Return if not a TeddySwap transaction
            if (poolInput is null || orderInput is null) return null;

            order = ProcessOrder(poolInput, orderInput, transactionEvent);
        }


        return order;
    }

    public Order? ProcessOrder(TxOutput poolInput, TxOutput orderInput, OuraTransactionEvent transactionEvent)
    {

        if (transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Context.TxIdx is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Outputs is not null)
        {

            OrderType orderType = _datumService.GetOrderType(orderInput.Address);
            List<OuraTxOutput> outputs = transactionEvent.Transaction.Outputs.ToList();

            byte[] poolDatumByteArray = _byteArrayService.HexToByteArray(poolInput.DatumCbor ?? "");
            byte[] orderDatumByteArray = _byteArrayService.HexToByteArray(orderInput.DatumCbor ?? "");

            PoolDatum? poolDatum = _datumService.CborToPoolDatum(CBORObject.DecodeFromBytes(poolDatumByteArray));

            OuraTxOutput? poolOutput = outputs[0];
            OuraTxOutput? rewardOutput = outputs[1];
            OuraTxOutput? batcherOutput = outputs[2];

            if (poolDatum is not null &&
                poolOutput is not null &&
                rewardOutput is not null &&
                batcherOutput is not null)
            {
                string assetX = string.Concat(poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
                string assetY = string.Concat(poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
                string assetLq = string.Concat(poolDatum.Lq.PolicyId, poolDatum.Lq.Name);
                string poolNft = string.Concat(poolDatum.Nft.PolicyId + poolDatum.Nft.Name);
                string orderBase = "";

                BigInteger reservesX = FindAsset(outputs[0], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
                BigInteger reservesY = FindAsset(outputs[0], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
                BigInteger liquidity = FindAsset(outputs[0], poolDatum.Lq.PolicyId, poolDatum.Lq.Name);

                BigInteger orderX = 0; // deposited X tokens
                BigInteger orderY = 0;  // deposited Y tokens
                BigInteger orderLq = 0; // received LQ tokens

                switch (orderType)
                {
                    case OrderType.Deposit:
                        orderX = FindAsset(orderInput, poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name); // deposited X tokens
                        orderY = FindAsset(orderInput, poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name); // deposited Y tokens
                        orderLq = FindAsset(outputs[1], poolDatum.Lq.PolicyId, poolDatum.Lq.Name); // received LQ tokens
                        break;
                    case OrderType.Redeem:
                        orderX = FindAsset(outputs[1], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name); // received X tokens
                        orderY = FindAsset(outputs[1], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name); // received Y tokens
                        orderLq = FindAsset(orderInput, poolDatum.Lq.PolicyId, poolDatum.Lq.Name); // deposited LQ tokens

                        break;
                    case OrderType.Swap:
                        SwapDatum? swapDatum = _datumService.CborToSwapDatum(CBORObject.DecodeFromBytes(orderDatumByteArray));

                        if (swapDatum is not null)
                        {
                            orderBase = swapDatum.Base.PolicyId + swapDatum.Base.Name;
                            bool isAssetXBase = assetX == orderBase;
                            orderX = isAssetXBase ?
                                FindAsset(orderInput, poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name) :
                                FindAsset(outputs[1], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
                            orderY = isAssetXBase ?
                                FindAsset(outputs[1], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name) :
                                FindAsset(orderInput, poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
                            orderLq = 0;
                        }
                        break;
                    case OrderType.Unknown:
                        _logger.LogInformation("Invalid order!");
                        break;
                }

                if (poolDatum is not null &&
                    rewardOutput is not null &&
                    rewardOutput.Address is not null &&
                    batcherOutput.Address is not null &&
                    transactionEvent.Context.Slot is not null)
                {
                    return new()
                    {
                        TxHash = transactionEvent.Context.TxHash,
                        Index = (ulong)transactionEvent.Context.TxIdx,
                        OrderType = orderType,
                        UserAddress = rewardOutput.Address,
                        BatcherAddress = batcherOutput.Address,
                        PoolDatum = poolDatumByteArray,
                        OrderDatum = orderDatumByteArray,
                        AssetX = assetX,
                        AssetY = assetY,
                        AssetLq = assetLq,
                        PoolNft = poolNft,
                        OrderBase = orderBase,
                        ReservesX = reservesX,
                        ReservesY = reservesY,
                        Liquidity = liquidity,
                        OrderX = orderX,
                        OrderY = orderY,
                        OrderLq = orderLq,
                        Slot = (ulong)transactionEvent.Context.Slot
                    };
                }
            }
        }

        return null;
    }

    public BigInteger FindAsset(OuraTxOutput output, string policyId, string name)
    {
        if (output.Assets is null) return 0;
        BigInteger amount;

        if (policyId == "lovelace")
        {
            amount = output.Amount is null ? 0 : (ulong)output.Amount;
        }
        else
        {
            OuraAsset? asset = output.Assets.Where(a => a.Policy == policyId && a.Asset == name).FirstOrDefault();
            amount = asset is not null && asset.Amount is not null ? (ulong)asset.Amount : 0;
        }

        return amount;
    }

    public BigInteger FindAsset(TxOutput output, string policyId, string name)
    {
        if (output.Assets is null) return 0;
        BigInteger amount;

        if (policyId == "lovelace")
        {
            amount = output.Amount;
        }
        else
        {
            Asset? asset = output.Assets.Where(a => a.PolicyId == policyId && a.Name == name).FirstOrDefault();
            amount = asset is null ? 0 : asset.Amount;
        }
        return amount;
    }
}