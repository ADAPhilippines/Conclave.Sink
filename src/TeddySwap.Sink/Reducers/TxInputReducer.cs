using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput)]
public class TxInputReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TxInputReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkCoreDbContext> _dbContextFactory;
    public TxInputReducer(
        ILogger<TxInputReducer> logger,
        IDbContextFactory<TeddySwapSinkCoreDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReduceAsync(OuraTxInput txInput)
    {
        if (txInput is not null &&
            txInput.TxHash is not null)
        {
            using TeddySwapSinkCoreDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
            Transaction? tx = await _dbContext.Transactions
                .Include(tx => tx.Block)
                .Where(tx => tx.Hash == txInput.TxHash)
                .FirstOrDefaultAsync();
            if (tx is not null)
            {
                TxOutput? txOutput = await _dbContext.TxOutputs
                    .Where(txOutput => txOutput.TxHash == txInput.TxHash && txOutput.Index == txInput.Index)
                    .FirstOrDefaultAsync();
                if (txOutput is not null)
                {
                    TxInput? input = await _dbContext.TxInputs
                        .Where(i => i.TxHash == txOutput.TxHash && i.TxOutputIndex == txOutput.Index)
                        .FirstOrDefaultAsync();

                    if (input is null)
                    {
                        await _dbContext.TxInputs.AddAsync(new()
                        {
                            TxHash = txInput.TxHash,
                            Transaction = tx,
                            TxOutputHash = txInput.TxHash,
                            TxOutputIndex = txInput.Index,
                            TxOutput = txOutput
                        });

                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }

    public async Task RollbackAsync(Block _) => await Task.CompletedTask;
}