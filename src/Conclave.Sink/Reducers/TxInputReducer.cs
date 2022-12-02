using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput)]
public class TxInputReducer : OuraReducerBase, ICoreReducer
{
    private readonly ILogger<TxInputReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public TxInputReducer(
        ILogger<TxInputReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReduceAsync(OuraTxInputEvent txInputEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (txInputEvent is not null &&
            txInputEvent.TxInput is not null &&
            txInputEvent.Context is not null &&
            txInputEvent.Context.Slot is not null && 
             txInputEvent.Context.TxHash is not null)
        {
            Block? block = await _dbContext.Block
                .Where(block => block.BlockHash == txInputEvent.Context.BlockHash).FirstOrDefaultAsync();
            TxOutput? txOutput = await _dbContext.TxOutput
                .Where(txOutput => txOutput.TxHash == txInputEvent.TxInput.TxHash && txOutput.Index == txInputEvent.TxInput.Index).FirstOrDefaultAsync();
            if (block is not null && txOutput is not null)
            {
                await _dbContext.TxInput.AddAsync(new()
                {
                    TxHash = txInputEvent.Context.TxHash,
                    TxOutputHash = txInputEvent.TxInput.TxHash,
                    TxOutputIndex = txInputEvent.TxInput.Index,
                    TxOutput = txOutput,
                    Block = block
                });
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    public async Task RollbackAsync(IEnumerable<Block> rollbackBlocks)
    {
        // No Implementation
        await Task.Run(() => { });
    }
}