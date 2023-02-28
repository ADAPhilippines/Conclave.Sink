using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using TeddySwap.Sink.Data;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Conclave.Sink.Reducers;

public class OrderReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IServiceProvider _serviceProvider;

    public OrderReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _serviceProvider = serviceProvider;
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

            Transaction? transaction = await _dbContext.Transactions
                .Include(t => t.Inputs)
                .ThenInclude(i => i.TxOutput)
                .Include(t => t.Outputs)
                .Where(t => t.Hash == transactionEvent.Context.TxHash)
                .FirstOrDefaultAsync();

            if (transaction is null) throw new NullReferenceException("Transaction does not exist!");

        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}