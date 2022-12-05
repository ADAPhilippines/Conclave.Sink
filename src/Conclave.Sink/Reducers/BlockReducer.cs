using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.Block)]
public class BlockReducer : OuraReducerBase, IOuraCoreReducer
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
            await RollbackBySlotAsync((ulong)blockEvent.Context.Slot);

            // New Context for this insert
            _dbContext = await _dbContextFactory.CreateDbContextAsync();
            await _dbContext.Block.AddAsync(new()
            {
                BlockNumber = (ulong)blockEvent.Context.BlockNumber,
                VrfKeyhash = HashUtility.Blake2b256(blockEvent.Block.VrfVkey.HexToByteArray()).ToStringHex(),
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

    public async Task RollbackBySlotAsync(ulong rollbackSlot)
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

                await Task.WhenAll(reducers
                    .Where
                    (
                        reducer => reducer is not IOuraCoreReducer
                    )
                    .Select((reducer) => Task.Run(async () =>
                    {
                        await reducer.HandleRollbackAsync(rollbackBlock);
                    }))
                );
                await this.HandleRollbackAsync(rollbackBlock);
            }

        }
    }
}