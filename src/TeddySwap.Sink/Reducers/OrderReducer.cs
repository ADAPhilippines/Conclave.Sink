using System.Numerics;
using System.Text;
using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using PeterO.Cbor2;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class OrderReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<OrderReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly OrderService _orderService;
    private readonly IServiceProvider _serviceProvider;
    private readonly TeddySwapSinkSettings _settings;

    public OrderReducer(
        ILogger<OrderReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        OrderService orderService,
        IServiceProvider serviceProvider,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _orderService = orderService;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraTransactionEvent transactionEvent)
    {
        if (transactionEvent is not null &&
            transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Fee is not null &&
            transactionEvent.Context.TxIdx is not null)
        {
            TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Block? block = await _dbContext.Blocks
                .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) throw new NullReferenceException("Block does not exist!");

            if (block.InvalidTransactions is null ||
                !block.InvalidTransactions.ToList().Contains((ulong)transactionEvent.Context.TxIdx))
            {
                Order? order = await _orderService.ProcessOrderAsync(transactionEvent);

                if (order is not null)
                {
                    order.Block = block;
                    order.Blockhash = block.BlockHash;

                    await _dbContext.Orders.AddAsync(order);

                    if (order.OrderType == OrderType.Swap)
                    {

                        await _dbContext.Prices.AddAsync(new Price()
                        {
                            TxHash = order.TxHash,
                            Index = order.Index,
                            Order = order,
                            PriceX = BigIntegerDivToDecimal(order.ReservesX, order.ReservesY, 6),
                            PriceY = BigIntegerDivToDecimal(order.ReservesY, order.ReservesX, 6),
                        });
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    private static decimal BigIntegerDivToDecimal(BigInteger x, BigInteger y, int precision)
    {

        var divResult = BigInteger.DivRem(x, y);

        StringBuilder result = new();
        result.Append(divResult.Quotient.ToString());

        if (divResult.Remainder != 0)
        {
            result.Append('.');

            for (int i = 0; i < precision; i++)
            {
                divResult.Remainder *= 10;
                result.Append(BigInteger.DivRem(divResult.Remainder, y).Quotient.ToString());

                if (BigInteger.DivRem(divResult.Remainder, y).Remainder == 0)
                {
                    break;
                }
            }
        }

        return decimal.Parse(result.ToString());
    }
    public async Task RollbackAsync(Block _) => await Task.CompletedTask;
}