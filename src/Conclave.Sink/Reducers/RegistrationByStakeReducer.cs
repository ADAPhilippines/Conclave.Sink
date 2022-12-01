using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.StakeRegistration)]
public class RegistrationByStakeReducer : OuraReducerBase
{
    private readonly ILogger<RegistrationByStakeReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public RegistrationByStakeReducer(ILogger<RegistrationByStakeReducer> logger, IDbContextFactory<ConclaveSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    public async Task ReduceAsync(OuraStakeRegistrationEvent stakeRegistrationEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (stakeRegistrationEvent is not null &&
            stakeRegistrationEvent.Context is not null &&
            stakeRegistrationEvent.StakeRegistration is not null &&
            stakeRegistrationEvent.StakeRegistration.Credential is not null &&
            stakeRegistrationEvent.Context.TxHash is not null)
        {
            await _dbContext.RegistrationByStake.AddAsync(new RegistrationByStake
            {
                StakeHash = string.IsNullOrEmpty(stakeRegistrationEvent.StakeRegistration.Credential.AddrKeyHash) ?
                    stakeRegistrationEvent.StakeRegistration.Credential.Scripthash :
                    stakeRegistrationEvent.StakeRegistration.Credential.AddrKeyHash,
                TxHash = stakeRegistrationEvent.Context.TxHash,
                TxIndex = stakeRegistrationEvent.Context.TxIdx ?? 0
            });

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        _logger.LogInformation("RegistrationByStake Rollback...");

    }
}