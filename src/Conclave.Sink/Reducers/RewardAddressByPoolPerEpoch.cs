using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration)]
public class RewardAddressByPoolPerEpochReducer : OuraReducerBase
{
    private readonly ILogger<RewardAddressByPoolPerEpochReducer> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    public RewardAddressByPoolPerEpochReducer(ILogger<RewardAddressByPoolPerEpochReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
    }
    public async Task ReduceAsync(OuraPoolRegistrationEvent poolRegistrationEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (poolRegistrationEvent is not null &&
            poolRegistrationEvent.Context is not null &&
            poolRegistrationEvent.PoolRegistration is not null &&
            poolRegistrationEvent.PoolRegistration.RewardAccount is not null &&
            poolRegistrationEvent.Context.Slot is not null)
        {
            ulong slot = (ulong)poolRegistrationEvent.Context.Slot;
            RewardAddressByPoolPerEpoch? entry = await _dbContext.RewardAddressByPoolPerEpoch
                .Where((rapp) => rapp.RewardAddress == poolRegistrationEvent.PoolRegistration.RewardAccount &&
                        rapp.Slot == slot && rapp.PoolId == poolRegistrationEvent.PoolRegistration.Operator)
                .FirstOrDefaultAsync();

            if (entry is not null) return;

            Block? block = await _dbContext.Block.Where(b => b.BlockHash == poolRegistrationEvent.Context.BlockHash).FirstOrDefaultAsync();

            await _dbContext.RewardAddressByPoolPerEpoch.AddAsync(new RewardAddressByPoolPerEpoch
            {
                PoolId = poolRegistrationEvent.PoolRegistration.Operator,
                RewardAddress = poolRegistrationEvent.PoolRegistration.RewardAccount,
                Slot = slot,
                Block = block
            });

            await _dbContext.SaveChangesAsync();
        }
    }


    public async Task RollbackAsync(Block rollbackBlock)
    {
        _logger.LogInformation("RegistrationByStake Rollback...");

    }
}
