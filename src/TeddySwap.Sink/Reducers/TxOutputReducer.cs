using TeddySwap.Sink.Data;
using TeddySwap.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TeddySwap.Sink.Models.Oura;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxOutput)]
public class TxOutputReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TxOutputReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public TxOutputReducer(
        ILogger<TxOutputReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReduceAsync(OuraTxOutputEvent txOutputEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (txOutputEvent is not null &&
        txOutputEvent.Context is not null &&
         txOutputEvent.TxOutput is not null &&
         txOutputEvent.Context.TxHash is not null &&
         txOutputEvent.Context.OutputIdx is not null &&
         txOutputEvent.Context.BlockHash is not null &&
         txOutputEvent.TxOutput.Amount is not null &&
         txOutputEvent.TxOutput.Address is not null)
        {
            Transaction? tx = await _dbContext.Transactions.Include(tx => tx.Block).Where(tx => tx.Hash == txOutputEvent.Context.TxHash).FirstOrDefaultAsync();

            if (tx is not null)
            {
                TxOutput newTxOutput = new()
                {
                    Amount = (ulong)txOutputEvent.TxOutput.Amount,
                    Address = txOutputEvent.TxOutput.Address,
                    Index = (ulong)txOutputEvent.Context.OutputIdx
                };

                newTxOutput = newTxOutput with { Transaction = tx, TxHash = tx.Hash };

                EntityEntry<TxOutput> insertResult = await _dbContext.TxOutputs.AddAsync(newTxOutput);
                await _dbContext.SaveChangesAsync();

                if (txOutputEvent.TxOutput.Assets is not null && txOutputEvent.TxOutput.Assets.Count() > 0)
                {
                    await _dbContext.AddRangeAsync(txOutputEvent.TxOutput.Assets.Select(ouraAsset =>
                    {
                        return new Asset
                        {
                            PolicyId = ouraAsset.Policy ?? string.Empty,
                            Name = ouraAsset.Asset ?? string.Empty,
                            Amount = ouraAsset.Amount ?? 0,
                            TxOutput = insertResult.Entity
                        };
                    }));
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}