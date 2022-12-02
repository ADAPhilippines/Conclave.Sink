using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxOutput)]
public class TxOutputReducer : OuraReducerBase, ICoreReducer
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
            Block? block = await _dbContext.Block.Where(block => block.BlockHash == txOutputEvent.Context.BlockHash).FirstOrDefaultAsync();
            if (block is not null)
            {
                await _dbContext.TxOutput.AddAsync(new()
                {
                    TxHash = txOutputEvent.Context.TxHash,
                    Index = (ulong)txOutputEvent.Context.OutputIdx,
                    Amount = (ulong)txOutputEvent.TxOutput.Amount,
                    Address = txOutputEvent.TxOutput.Address,
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