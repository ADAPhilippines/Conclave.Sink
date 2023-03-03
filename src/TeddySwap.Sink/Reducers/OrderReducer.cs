using System.Numerics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.Enums;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class OrderReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<OrderReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    private readonly OrderService _orderService;

    public OrderReducer(
        ILogger<OrderReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        OrderService orderService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _orderService = orderService;
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
                Order? existingOrder = await _dbContext.Orders
                    .Where(o => o.TxHash == transactionEvent.Context.TxHash && o.Index == transactionEvent.Context.TxIdx)
                    .FirstOrDefaultAsync();

                if (existingOrder is not null) return;

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