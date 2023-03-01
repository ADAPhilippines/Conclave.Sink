using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PeterO.Cbor2;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Services;

public class OrderService
{

    private readonly TeddySwapSinkSettings _settings;
    private readonly DatumService _datumService;
    private readonly CardanoService _cardanoService;
    private readonly ILogger<OrderService> _logger;
    private IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;

    public OrderService(
        DatumService datumService,
        IOptions<TeddySwapSinkSettings> settings,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        ILogger<OrderService> logger)
    {
        _settings = settings.Value;
        _datumService = datumService;
        _cardanoService = cardanoService;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Order?> ProcessOrderAsync(OuraTransactionEvent transactionEvent)
    {
        TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Inputs is not null &&
            transactionEvent.Transaction.Outputs is not null)
        {
            List<string> inputRefs = transactionEvent.Transaction.Inputs.Select(i => i.TxHash + i.Index).ToList();
            List<TxOutput>? inputs = await _dbContext.TxOutputs.Where(o => inputRefs.Contains(o.TxHash + o.Index)).ToListAsync();

            List<string> validators = new()
                {
                    _settings.DepositAddress,
                    _settings.SwapAddress,
                    _settings.RedeemAddress
                };

            // Find Validator Utxos
            TxOutput? poolInput = inputs.Where(i => i.Address == _settings.PoolAddress).FirstOrDefault();
            TxOutput? orderInput = inputs.Where(i => validators.Contains(i.Address)).FirstOrDefault();

            // Return if not a TeddySwap transaction
            if (poolInput is null || orderInput is null) return null;

            OrderType orderType = _datumService.GetOrderType(orderInput.Address);

            Order? order = orderType switch
            {
                OrderType.Deposit => processDepositOrder(poolInput, orderInput, transactionEvent),
                OrderType.Redeem => null,
                OrderType.Swap => null,
                _ => null,
            };
        }


        return new Order();
    }

    public Order? processDepositOrder(TxOutput poolInput, TxOutput orderInput, OuraTransactionEvent transactionEvent)
    {

        if (transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Context.TxIdx is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Outputs is not null)
        {
            List<OuraTxOutput> outputs = transactionEvent.Transaction.Outputs.ToList();
            DepositDatum? orderDatum = _datumService.CborToDepositDatum(CBORObject.DecodeFromBytes(orderInput.InlineDatum));
            PoolDatum? poolDatum = _datumService.CborToPoolDatum(CBORObject.DecodeFromBytes(poolInput.InlineDatum));
            OuraTxOutput? poolOutput = outputs[0];
            OuraTxOutput? rewardOutput = outputs[1];
            OuraTxOutput? batcherOutput = outputs[2];

            string assetX = poolDatum.ReserveX.PolicyId + poolDatum.ReserveX.Name;
            string assetY = poolDatum.ReserveY.PolicyId + poolDatum.ReserveY.Name;
            string assetLq = poolDatum.Lq.PolicyId + poolDatum.Lq.Name;
            string poolNft = poolDatum.Nft.PolicyId + poolDatum.Nft.Name;

            BigInteger reservesX = FindAsset(outputs[0], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
            BigInteger reservesY = FindAsset(outputs[0], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
            BigInteger liquidity = FindAsset(outputs[0], poolDatum.Lq.PolicyId, poolDatum.Lq.Name);
            BigInteger orderX = FindAsset(orderInput, poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
            BigInteger orderY = FindAsset(orderInput, poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
            BigInteger orderLq = FindAsset(outputs[1], poolDatum.Lq.PolicyId, poolDatum.Lq.Name);

            if (orderDatum is null ||
                poolDatum is null ||
                rewardOutput is null ||
                rewardOutput.Address is null ||
                batcherOutput.Address is null ||
                transactionEvent.Context.Slot is null) return null;

            return new()
            {
                TxHash = transactionEvent.Context.TxHash,
                Index = (ulong)transactionEvent.Context.TxIdx,
                OrderType = OrderType.Deposit,
                RewardAddress = rewardOutput.Address,
                BatcherAddress = batcherOutput.Address,
                PoolDatum = poolInput.InlineDatum,
                OrderDatum = orderInput.InlineDatum,
                AssetX = assetX,
                AssetY = assetY,
                AssetLq = assetLq,
                PoolNft = poolNft,
                OrderBase = "",
                ReservesX = reservesX,
                ReservesY = reservesY,
                Liquidity = liquidity,
                Slot = (ulong)transactionEvent.Context.Slot
            };

        }

        return null;
    }

    public Order? processRedeemOrder(TxOutput poolInput, TxOutput orderInput, OuraTransactionEvent transactionEvent)
    {

        if (transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Context.TxIdx is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Outputs is not null)
        {
            List<OuraTxOutput> outputs = transactionEvent.Transaction.Outputs.ToList();
            RedeemDatum? orderDatum = _datumService.CborToRedeemDatum(CBORObject.DecodeFromBytes(orderInput.InlineDatum));
            PoolDatum? poolDatum = _datumService.CborToPoolDatum(CBORObject.DecodeFromBytes(poolInput.InlineDatum));
            OuraTxOutput? poolOutput = outputs[0];
            OuraTxOutput? rewardOutput = outputs[1];
            OuraTxOutput? batcherOutput = outputs[2];

            string assetX = poolDatum.ReserveX.PolicyId + poolDatum.ReserveX.Name;
            string assetY = poolDatum.ReserveY.PolicyId + poolDatum.ReserveY.Name;
            string assetLq = poolDatum.Lq.PolicyId + poolDatum.Lq.Name;
            string poolNft = poolDatum.Nft.PolicyId + poolDatum.Nft.Name;

            BigInteger reservesX = FindAsset(outputs[0], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
            BigInteger reservesY = FindAsset(outputs[0], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
            BigInteger liquidity = FindAsset(outputs[0], poolDatum.Lq.PolicyId, poolDatum.Lq.Name);
            BigInteger orderX = FindAsset(outputs[1], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
            BigInteger orderY = FindAsset(outputs[1], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
            BigInteger orderLq = FindAsset(orderInput, poolDatum.Lq.PolicyId, poolDatum.Lq.Name);

            if (orderDatum is null ||
                poolDatum is null ||
                rewardOutput is null ||
                rewardOutput.Address is null ||
                batcherOutput.Address is null ||
                transactionEvent.Context.Slot is null) return null;

            return new()
            {
                TxHash = transactionEvent.Context.TxHash,
                Index = (ulong)transactionEvent.Context.TxIdx,
                OrderType = OrderType.Deposit,
                RewardAddress = rewardOutput.Address,
                BatcherAddress = batcherOutput.Address,
                PoolDatum = poolInput.InlineDatum,
                OrderDatum = orderInput.InlineDatum,
                AssetX = assetX,
                AssetY = assetY,
                AssetLq = assetLq,
                PoolNft = poolNft,
                OrderBase = "",
                ReservesX = reservesX,
                ReservesY = reservesY,
                Liquidity = liquidity,
                Slot = (ulong)transactionEvent.Context.Slot
            };

        }

        return null;
    }

    public Order? processSwapOrder(TxOutput poolInput, TxOutput orderInput, OuraTransactionEvent transactionEvent)
    {

        if (transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Context.TxIdx is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Outputs is not null)
        {
            List<OuraTxOutput> outputs = transactionEvent.Transaction.Outputs.ToList();
            SwapDatum? orderDatum = _datumService.CborToSwapDatum(CBORObject.DecodeFromBytes(orderInput.InlineDatum));
            PoolDatum? poolDatum = _datumService.CborToPoolDatum(CBORObject.DecodeFromBytes(poolInput.InlineDatum));
            OuraTxOutput? poolOutput = outputs[0];
            OuraTxOutput? rewardOutput = outputs[1];
            OuraTxOutput? batcherOutput = outputs[2];

            string assetX = poolDatum.ReserveX.PolicyId + poolDatum.ReserveX.Name;
            string assetY = poolDatum.ReserveY.PolicyId + poolDatum.ReserveY.Name;
            string assetLq = poolDatum.Lq.PolicyId + poolDatum.Lq.Name;
            string poolNft = poolDatum.Nft.PolicyId + poolDatum.Nft.Name;
            string orderBase = orderDatum.Base.PolicyId + orderDatum.Base.Name;

            BigInteger reservesX = FindAsset(outputs[0], poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
            BigInteger reservesY = FindAsset(outputs[0], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
            BigInteger liquidity = FindAsset(outputs[0], poolDatum.Lq.PolicyId, poolDatum.Lq.Name);
            BigInteger orderX = FindAsset(orderInput, poolDatum.ReserveX.PolicyId, poolDatum.ReserveX.Name);
            BigInteger orderY = FindAsset(outputs[1], poolDatum.ReserveY.PolicyId, poolDatum.ReserveY.Name);
            BigInteger orderLq = FindAsset(orderInput, poolDatum.Lq.PolicyId, poolDatum.Lq.Name);

            if (orderDatum is null ||
                poolDatum is null ||
                rewardOutput is null ||
                rewardOutput.Address is null ||
                batcherOutput.Address is null ||
                transactionEvent.Context.Slot is null) return null;

            return new()
            {
                TxHash = transactionEvent.Context.TxHash,
                Index = (ulong)transactionEvent.Context.TxIdx,
                OrderType = OrderType.Deposit,
                RewardAddress = rewardOutput.Address,
                BatcherAddress = batcherOutput.Address,
                PoolDatum = poolInput.InlineDatum,
                OrderDatum = orderInput.InlineDatum,
                AssetX = assetX,
                AssetY = assetY,
                AssetLq = assetLq,
                PoolNft = poolNft,
                OrderBase = "",
                ReservesX = reservesX,
                ReservesY = reservesY,
                Liquidity = liquidity,
                Slot = (ulong)transactionEvent.Context.Slot
            };

        }

        return null;
    }

    public BigInteger FindAsset(OuraTxOutput output, string policyId, string name)
    {
        if (output.Assets is null) return 0;

        OuraAsset? asset = output.Assets.Where(a => a.Policy == policyId && a.Asset == name).FirstOrDefault();
        return asset is not null && asset.Amount is not null ? (ulong)asset.Amount : 0;
    }

    public BigInteger FindAsset(TxOutput output, string policyId, string name)
    {
        if (output.Assets is null) return 0;

        Asset? asset = output.Assets.Where(a => a.PolicyId == policyId && a.Name == name).FirstOrDefault();
        return asset is not null ? asset.Amount : 0;
    }
}