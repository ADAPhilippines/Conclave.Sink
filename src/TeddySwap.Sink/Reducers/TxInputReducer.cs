using System.Text.Json;
using TeddySwap.Sink.Data;
using TeddySwap.Common.Models;
using Microsoft.EntityFrameworkCore;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput)]
public class TxInputReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TxInputReducer> _logger;
    private IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    public TxInputReducer(
        ILogger<TxInputReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReduceAsync(OuraTxInputEvent txInputEvent)
    {
        using TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (txInputEvent is not null &&
            txInputEvent.TxInput is not null &&
            txInputEvent.Context is not null &&
            txInputEvent.Context.Slot is not null &&
             txInputEvent.Context.TxHash is not null)
        {
            TxOutput? txOutput = await _dbContext.TxOutputs
                .Where(txOutput => txOutput.TxHash == txInputEvent.TxInput.TxHash && txOutput.Index == txInputEvent.TxInput.Index).FirstOrDefaultAsync();
            Transaction? tx = await _dbContext.Transactions.Include(tx => tx.Block).Where(tx => tx.Hash == txInputEvent.Context.TxHash).FirstOrDefaultAsync();
            if (tx is not null)
            {
                if (txOutput is not null)
                {
                    await _dbContext.TxInputs.AddAsync(new()
                    {
                        TxHash = txInputEvent.Context.TxHash,
                        Transaction = tx,
                        TxOutputHash = txInputEvent.TxInput.TxHash,
                        TxOutputIndex = txInputEvent.TxInput.Index,
                        TxOutput = txOutput
                    });
                }
                // else
                // {
                //     await _dbContext.TxInputs.AddAsync(new()
                //     {
                //         TxHash = txInputEvent.Context.TxHash,
                //         Transaction = tx,
                //         // GENESIS TX HACK
                //         TxOutput = new TxOutput
                //         {
                //             Transaction = new Transaction
                //             {
                //                 Hash = $"GENESIS_{tx.Hash}_{txInputEvent.Fingerprint}",
                //                 Block = tx.Block
                //             },
                //             Address = "GENESIS"
                //         }
                //     });
                // }
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}