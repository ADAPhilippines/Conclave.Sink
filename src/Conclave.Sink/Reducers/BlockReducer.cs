using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.Block)]
public class BlockReducer : OuraReducerBase
{
    private readonly ILogger<BlockReducer> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IServiceProvider _serviceProvider;

    public BlockReducer(
        ILogger<BlockReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _serviceProvider = serviceProvider;
    }

    public async Task ReduceAsync(OuraBlockEvent blockEvent)
    {
        ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (blockEvent.Context is not null &&
            blockEvent.Context.BlockNumber is not null &&
            blockEvent.Context.Slot is not null &&
            blockEvent.Context.BlockHash is not null)
        {
            await RollbackAsync((ulong)blockEvent.Context.Slot);

            // New Context for this insert
            _dbContext = await _dbContextFactory.CreateDbContextAsync();
            await _dbContext.Block.AddAsync(new()
            {
                BlockNumber = (ulong)blockEvent.Context.BlockNumber,
                Slot = (ulong)blockEvent.Context.Slot,
                BlockHash = blockEvent.Context.BlockHash,
                Epoch = _cardanoService.CalculateEpochBySlot((ulong)blockEvent.Context.Slot)
            });
            await _dbContext.SaveChangesAsync();
            await _dbContext.DisposeAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        _dbContext.Block.Remove(rollbackBlock);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RollbackAsync(ulong rollbackSlot)
    {
        ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        ulong currentTipSlot = await _dbContext.Block.AnyAsync() ? await _dbContext.Block.MaxAsync(block => block.Slot) : 0;

        // Check if current database tip clashes with the current tip oura is pushing
        // if so then we should rollback and insert the new tip oura is pushing
        if (rollbackSlot <= currentTipSlot)
        {
            List<Block> blocksToRollback = await _dbContext.Block
                .Where(block => block.Slot >= rollbackSlot)
                .OrderByDescending(block => block.Slot)
                .ToListAsync();

            IEnumerable<IOuraReducer> reducers = _serviceProvider.GetServices<IOuraReducer>();

            foreach (Block rollbackBlock in blocksToRollback)
            {
                _logger.LogInformation($"Rolling back Block No: {rollbackBlock.BlockNumber}, Block Hash: {rollbackBlock.BlockHash}");

                IEnumerable<IOuraReducer> nonCoreReducers = reducers
                    .Where
                    (
                        reducer => reducer is not BlockReducer &&
                            reducer is not TxOutputReducer &&
                            reducer is not TxInputReducer
                    );

                foreach (IOuraReducer rollbackTask in nonCoreReducers) await rollbackTask.HandleRollbackAsync(rollbackBlock);

                TxOutputReducer? txOutputReducer = reducers.Where(reducer => reducer is TxOutputReducer).ToList().FirstOrDefault() as TxOutputReducer;
                TxInputReducer? txInputReducer = reducers.Where(reducer => reducer is TxInputReducer).ToList().FirstOrDefault() as TxInputReducer;
                if (txOutputReducer is not null && txInputReducer is not null)
                {
                    await txOutputReducer.HandleRollbackAsync(rollbackBlock);
                    await txInputReducer.HandleRollbackAsync(rollbackBlock);
                }
                await this.HandleRollbackAsync(rollbackBlock);
            }
        }
    }
}