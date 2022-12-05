using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRetirement)]
public class PoolRetirementReducer : OuraReducerBase
{
    private readonly ILogger<PoolRetirementReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private CardanoService _cardanoService;
    public PoolRetirementReducer(
        ILogger<PoolRetirementReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        OuraPoolRetirementEvent? poolRetirementEvent = ouraEvent as OuraPoolRetirementEvent;
        if (poolRetirementEvent is not null &&
            poolRetirementEvent.Context is not null &&
            poolRetirementEvent.PoolRetirement is not null &&
            poolRetirementEvent.PoolRetirement.Pool is not null &&
            poolRetirementEvent.PoolRetirement.Epoch is not null)
        {
            Block? block = await _dbContext.Block
                .Where(block => block.BlockHash == poolRetirementEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is not null)
            {
                await _dbContext.PoolRetirement.AddAsync(new()
                {
                    Pool = poolRetirementEvent.PoolRetirement.Pool,
                    Epoch = (ulong)poolRetirementEvent.PoolRetirement.Epoch,
                    Block = block
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<PoolRetirement> rollbackEntriesList = await _dbContext.PoolRetirement
            .Where(pr => pr.Block == rollbackBlock)
            .ToListAsync();

        if (rollbackEntriesList is not null &&
            rollbackEntriesList.Count is not 0)
            _dbContext.RemoveRange(rollbackEntriesList);

        await _dbContext.SaveChangesAsync();
    }
}