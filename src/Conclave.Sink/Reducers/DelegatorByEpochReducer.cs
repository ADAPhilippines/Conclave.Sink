using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.StakeDelegation)]
public class DelegatorByEpochReducer : OuraReducerBase
{

    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public DelegatorByEpochReducer(IDbContextFactory<ConclaveSinkDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    public async Task ReduceAsync(OuraStakeDelegationEvent stakeDelegationEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (stakeDelegationEvent is not null &&
            stakeDelegationEvent.Context is not null &&
            stakeDelegationEvent.StakeDelegation is not null &&
            stakeDelegationEvent.StakeDelegation.Credential is not null &&
            stakeDelegationEvent.StakeDelegation.PoolHash is not null &&
            stakeDelegationEvent.Context.Slot is not null)
        {
            Block? block = await _dbContext.Block.Where(b => b.BlockHash == stakeDelegationEvent.Context.BlockHash).FirstOrDefaultAsync();

            if (block is null) return;

            string stakeAddress = string.IsNullOrEmpty(stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash) ?
                                stakeDelegationEvent.StakeDelegation.Credential.Scripthash :
                                stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash;

            DelegatorByEpoch? entry = await _dbContext.DelegatorByEpoch.Include(dbe => dbe.Block)
                                .Where((dbe) => dbe.StakeAddress == stakeAddress &&
                                        dbe.Slot == block.Slot &&
                                        dbe.PoolHash == stakeDelegationEvent.StakeDelegation.PoolHash)
                                .FirstOrDefaultAsync();

            if (entry is not null &&
                entry.Block is not null &&
                entry.Block.BlockHash == stakeDelegationEvent.Context.BlockHash) return;


            await _dbContext.DelegatorByEpoch.AddAsync(new()
            {
                StakeAddress = string.IsNullOrEmpty(stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash) ?
                                stakeDelegationEvent.StakeDelegation.Credential.Scripthash :
                                stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash,
                PoolHash = stakeDelegationEvent.StakeDelegation.PoolHash,
                Slot = block.Slot,
                Block = block
            });

            await _dbContext.SaveChangesAsync();
        }
    }
}