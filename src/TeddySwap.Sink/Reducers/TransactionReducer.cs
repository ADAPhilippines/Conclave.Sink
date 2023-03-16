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

    public async Task ReduceAsync(OuraTransaction transaction)
    {
        try
        {
            if (transaction is not null &&
                transaction.Context is not null &&
                transaction.Fee is not null &&
                transaction.Hash is not null)
            {
                using TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

                Block? block = await _dbContext.Blocks
                    .Where(b => b.BlockHash == transaction.Context.BlockHash)
                    .FirstOrDefaultAsync();

                if (block is null) throw new NullReferenceException("Block does not exist!");

                Transaction? existingTransaction = await _dbContext.Transactions
                    .Where(t => t.Hash == transaction.Hash && t.Index == (ulong)transaction.Index)
                    .FirstOrDefaultAsync();

                if (existingTransaction is not null) return;

                Transaction newTransaction = new()
                {
                    Hash = transaction.Hash,
                    Fee = (ulong)transaction.Fee,
                    Index = (ulong)transaction.Index,
                    Block = block,
                    Blockhash = block.BlockHash
                };

                await _dbContext.Transactions.AddAsync(newTransaction);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch
        {
            _logger.LogInformation("Block insert failed...");
        }
    }
    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}