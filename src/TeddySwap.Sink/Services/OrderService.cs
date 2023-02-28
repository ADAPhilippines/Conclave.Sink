using Microsoft.Extensions.Options;
using PeterO.Cbor2;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Models;

namespace TeddySwap.Sink.Services;

public class OrderService
{

    private readonly TeddySwapSinkSettings _settings;
    private readonly DatumService _datumService;
    public OrderService(
        DatumService datumService,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _settings = settings.Value;
        _datumService = datumService;
    }

    public Order? ProcessOrder(Transaction transaction)
    {

        List<TxOutput>? inputs = transaction.Inputs.Select(i => i.TxOutput).ToList();
        List<TxOutput>? outputs = transaction.Outputs.ToList();

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

        var datum = orderType switch
        {
            OrderType.Deposit => _datumService.CborToDepositDatum(CBORObject.DecodeFromBytes(orderInput.InlineDatum))
        }

        // public record Order
        // {
        //     public string TxHash { get; init; } = string.Empty; ./
        //     public ulong Index { get; init; } ./
        //     public OrderType OrderType { get; init; } ./
        //     public string datum { get; init; } = string.Empty;
        //     public string RewardAddress { get; init; } = string.Empty;
        //     public string BatcherAddress { get; init; } = string.Empty;
        //     public string AssetX { get; init; } = string.Empty;
        //     public string AssetY { get; init; } = string.Empty;
        //     public string AssetLq { get; init; } = string.Empty;
        //     public string PoolNft { get; init; } = string.Empty;
        //     public BigInteger ReservesX { get; init; }
        //     public BigInteger ReservesY { get; init; }
        //     public BigInteger Liquidity { get; init; }
        //     public BigInteger OrderX { get; init; }
        //     public BigInteger OrderY { get; init; }
        //     public BigInteger OrderLq { get; init; }
        //     public ulong Slot { get; init; }
        // }

        return new Order();
    }
}