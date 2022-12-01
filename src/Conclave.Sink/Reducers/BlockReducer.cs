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
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (blockEvent.Context is not null &&
            blockEvent.Context.BlockNumber is not null &&
            blockEvent.Context.Slot is not null &&
            blockEvent.Context.BlockHash is not null)
        {
            ulong currentTipSlot = await _dbContext.Block.AnyAsync() ? await _dbContext.Block.MaxAsync(block => block.Slot) : 0;

            // Check if current database tip clashes with the current tip oura is pushing
            // if so then we should rollback and insert the new tip oura is pushing
            if (blockEvent.Context.Slot <= currentTipSlot)
            {
                List<Block> blocksToRollback = await _dbContext.Block
                    .Where(block => block.Slot >= blockEvent.Context.Slot)
                    .OrderByDescending(block => block.Slot)
                    .ToListAsync();

                IEnumerable<IOuraReducer> reducers = _serviceProvider.GetServices<IOuraReducer>().Where(
                    reducer => reducer is not BlockReducer && reducer is not TxOutputReducer
                );

                foreach (Block rollbackBlock in blocksToRollback)
                {
                    _logger.LogInformation($"Rolling back Block No: {rollbackBlock.BlockNumber}, Block Hash: {rollbackBlock.BlockHash}");
                    await Task.WhenAll(reducers.Select(reducer => reducer.HandleRollbackAsync(rollbackBlock)));
                    await _serviceProvider.GetService<TxOutputReducer>()?.HandleRollbackAsync(rollbackBlock)!;
                    await this.HandleRollbackAsync(rollbackBlock);
                }
            }

            await _dbContext.Block.AddAsync(new()
            {
                BlockNumber = (ulong)blockEvent.Context.BlockNumber,
                Slot = (ulong)blockEvent.Context.Slot,
                BlockHash = blockEvent.Context.BlockHash,
                Epoch = _cardanoService.CalculateEpochBySlot((ulong)blockEvent.Context.Slot)
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        _dbContext.Block.Remove(rollbackBlock);
        await _dbContext.SaveChangesAsync();
    }
}