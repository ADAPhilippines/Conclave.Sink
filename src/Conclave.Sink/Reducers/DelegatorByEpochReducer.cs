using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.StakeDelegation)]
public class DelegatorByEpochReducer : OuraReducerBase
{
    private readonly ILogger<DelegatorByEpochReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly ConclaveSinkSettings _settings;
    public DelegatorByEpochReducer(ILogger<DelegatorByEpochReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        IOptions<ConclaveSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _cardanoService = cardanoService;
    }
    public async Task ReduceAsync(OuraStakeDelegationEvent stakeDelegationEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (stakeDelegationEvent is not null &&
            stakeDelegationEvent.Context is not null &&
            stakeDelegationEvent.StakeDelegation is not null &&
            stakeDelegationEvent.StakeDelegation.Credential is not null &&
            stakeDelegationEvent.StakeDelegation.PoolHash is not null &&
            stakeDelegationEvent.Context.TxIdx is not null &&
            stakeDelegationEvent.Context.TxHash is not null)
        {
            Block? block = await _dbContext.Block.Where(b => b.BlockHash == stakeDelegationEvent.Context.BlockHash).FirstOrDefaultAsync();

            if (block is null) return;

            string stakeKeyHash = string.IsNullOrEmpty(stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash) ?
                stakeDelegationEvent.StakeDelegation.Credential.Scripthash :
                stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash;

            string stakeAddress = AddressUtility.GetRewardAddress(stakeKeyHash.HexToByteArray(), _settings.NetworkType).ToString();
            string poolId = _cardanoService.PoolHashToBech32(stakeDelegationEvent.StakeDelegation.PoolHash);

            DelegatorByEpoch? entry = await _dbContext.DelegatorByEpoch.Include(dbe => dbe.Block)
                .Where((dbe) => dbe.StakeAddress == stakeAddress &&
                    dbe.TxHash == stakeDelegationEvent.Context.TxHash &&
                    dbe.PoolId == poolId &&
                    dbe.TxIndex == (ulong)stakeDelegationEvent.Context.TxIdx)
                .FirstOrDefaultAsync();

            if (entry is not null &&
                entry.Block is not null &&
                entry.Block.BlockHash == stakeDelegationEvent.Context.BlockHash) return;


            await _dbContext.DelegatorByEpoch.AddAsync(new()
            {
                StakeAddress = stakeAddress,
                PoolId = poolId,
                TxHash = stakeDelegationEvent.Context.TxHash,
                TxIndex = (ulong)stakeDelegationEvent.Context.TxIdx,
                Block = block
            });

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        IEnumerable<DelegatorByEpoch>? delegatorByEpoch = _dbContext.DelegatorByEpoch.Include(dbe => dbe.Block)
            .Where(dbe => dbe.Block!.BlockHash == rollbackBlock.BlockHash);
        if (delegatorByEpoch is null) return;
        _dbContext.DelegatorByEpoch.RemoveRange(delegatorByEpoch);
        await _dbContext.SaveChangesAsync();
    }
}