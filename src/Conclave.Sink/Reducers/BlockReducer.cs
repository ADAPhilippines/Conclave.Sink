using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.Block)]
public class BlockReducer : OuraReducerBase
{
    private readonly ILogger<BlockReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private CardanoService _cardanoService;
    public BlockReducer(
        ILogger<BlockReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraBlockEvent blockEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (blockEvent.Context is not null &&
            blockEvent.Context.BlockNumber is not null &&
            blockEvent.Context.Slot is not null &&
            blockEvent.Context.BlockHash is not null)
        {
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

    public new async Task RollbackAsync(OuraBlockEvent txOutputEvent)
    {

    }
}