using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class TransactionReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;


    public TransactionReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
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
            using TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Block? block = await _dbContext.Blocks
                .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) throw new NullReferenceException("Block does not exist!");

            Transaction? existingTransaction = await _dbContext.Transactions
                .Where(t => t.Hash == transactionEvent.Context.TxHash && t.Index == transactionEvent.Context.TxIdx)
                .FirstOrDefaultAsync();

            if (existingTransaction is not null) return;

            Transaction transaction = new()
            {
                Hash = transactionEvent.Context.TxHash,
                Fee = (ulong)transactionEvent.Transaction.Fee,
                Index = (ulong)transactionEvent.Context.TxIdx,
                Block = block,
                Blockhash = block.BlockHash
            };

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}