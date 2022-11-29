using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxOutput)]
public class AddressByStakeReducer : OuraReducerBase
{

    private readonly ILogger<AddressByStakeReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public AddressByStakeReducer(
        ILogger<AddressByStakeReducer> logger,
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
            await _dbContext.TxOutput.AddAsync(new()
            {
                TxHash = txOutputEvent.Context.TxHash,
                Index = (ulong)txOutputEvent.Context.OutputIdx,
                Amount = (ulong)txOutputEvent.TxOutput.Amount,
                Address = txOutputEvent.TxOutput.Address,
                Block = await _dbContext.Block.Where(block => block.BlockHash == txOutputEvent.Context.BlockHash).FirstOrDefaultAsync()
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public new async Task RollbackAsync(OuraTxOutputEvent txOutputEvent)
    {

    }
}