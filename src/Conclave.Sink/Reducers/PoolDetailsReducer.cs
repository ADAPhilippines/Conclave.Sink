using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration, OuraVariant.PoolRetirement)]

public class PoolDetailsReducer : OuraReducerBase
{
    private readonly ILogger<TxOutputReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private CardanoService _cardanoService;
    public PoolDetailsReducer(
        ILogger<TxOutputReducer> logger,
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
        await (ouraEvent.Variant switch
        {
            OuraVariant.PoolRegistration => Task.Run(async () =>
            {
                OuraPoolRegistrationEvent? poolRegistrationEvent = ouraEvent as OuraPoolRegistrationEvent;
                if (poolRegistrationEvent is not null &&
                    poolRegistrationEvent.PoolRegistration is not null &&
                    poolRegistrationEvent.PoolRegistration.PoolMetadata is not null &&
                    poolRegistrationEvent.Context is not null &&
                    poolRegistrationEvent.Context.Slot is not null &&
                    poolRegistrationEvent.Context.BlockHash is not null)
                {
                    Block? block = await _dbContext.Block.Where(block => block.BlockHash == poolRegistrationEvent.Context.BlockHash).FirstOrDefaultAsync();

                    string? poolMetadataJSON = await GetJsonFromURL(poolRegistrationEvent.PoolRegistration.PoolMetadata);

                    if (poolMetadataJSON is not null &&
                        block is not null &&
                        poolRegistrationEvent.PoolRegistration.PoolMetadata is not null &&
                        poolRegistrationEvent.Context.TxHash is not null)
                    {
                        await _dbContext.Pools.AddAsync(new()
                        {
                            Operator = poolRegistrationEvent.PoolRegistration.Operator,
                            VRFKeyHash = poolRegistrationEvent.PoolRegistration.VRFKeyHash,
                            Pledge = poolRegistrationEvent.PoolRegistration.Pledge,
                            Cost = poolRegistrationEvent.PoolRegistration.Cost,
                            Margin = poolRegistrationEvent.PoolRegistration.Margin,
                            RewardAccount = poolRegistrationEvent.PoolRegistration.RewardAccount,
                            PoolOwners = poolRegistrationEvent.PoolRegistration.PoolOwners,
                            Relays = poolRegistrationEvent.PoolRegistration.Relays,
                            PoolMetadata = poolMetadataJSON,
                            Block = block,
                            TxHash = poolRegistrationEvent.Context.TxHash,
                        });

                        await _dbContext.SaveChangesAsync();
                    }
                }
            }),
            OuraVariant.PoolRetirement => Task.Run(async () =>
            {
                OuraPoolRetirementEvent? poolRetirementEvent = ouraEvent as OuraPoolRetirementEvent;
                if (poolRetirementEvent is not null &&
                    poolRetirementEvent.PoolRetirement is not null &&
                    poolRetirementEvent.PoolRetirement.Pool is not null)
                {
                    List<PoolDetails> poolEntries = await _dbContext.Pools
                        .Where(pool => pool.Operator == poolRetirementEvent.PoolRetirement.Pool)
                        .ToListAsync();

                    if (poolEntries.Count is not 0)
                    {
                        _dbContext.Pools.RemoveRange(poolEntries);

                        await _dbContext.SaveChangesAsync();
                    }
                }
            }),
            _ => Task.Run(() => { })
        });
    }

    public async Task<string?> GetJsonFromURL(string metaDataURL)
    {
        try
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(metaDataURL);
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<PoolDetails> rollbackEntry = await _dbContext.Pools.Where(p => p.Block == rollbackBlock).ToListAsync();

        if (rollbackEntry is not null && rollbackEntry.Count is not 0)
            _dbContext.RemoveRange(rollbackEntry);

        await _dbContext.SaveChangesAsync();
    }
}