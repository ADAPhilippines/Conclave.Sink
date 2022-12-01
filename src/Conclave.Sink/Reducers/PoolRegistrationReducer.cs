using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration)]
public class PoolRegistrationReducer : OuraReducerBase
{
    private readonly ILogger<TxOutputReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public PoolRegistrationReducer(
        ILogger<TxOutputReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReduceAsync(OuraPoolRegistrationEvent poolRegistrationEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (poolRegistrationEvent is not null &&
            poolRegistrationEvent.Context is not null &&
            poolRegistrationEvent.PoolRegistration is not null)
        {
            Pool? poolEntry = await _dbContext.Pools
                .Where(pool => (pool.VRFKeyHash == poolRegistrationEvent.PoolRegistration.VRFKeyHash) && (pool.Operator == poolRegistrationEvent.PoolRegistration.Operator))
                .FirstOrDefaultAsync();

            if (poolEntry is null)
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
                    PoolMetadata = poolRegistrationEvent.PoolRegistration.PoolMetadata
                });
            }
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        _logger.LogInformation("Pool Registration Rollback");
    }
}