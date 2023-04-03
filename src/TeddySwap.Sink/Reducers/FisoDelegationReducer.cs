using System.Text.Json;
using CardanoSharp.Wallet.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.StakeDelegation)]
public class FisoDelegationReducer : OuraReducerBase
{
    private readonly ILogger<FisoDelegationReducer> _logger;
    private readonly IDbContextFactory<TeddySwapFisoSinkDbContext> _dbContextFactory;
    private readonly IDbContextFactory<CardanoDbSyncContext> _cardanoDbSyncContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly TeddySwapSinkSettings _settings;

    public FisoDelegationReducer(
        ILogger<FisoDelegationReducer> logger,
        IDbContextFactory<TeddySwapFisoSinkDbContext> dbContextFactory,
        IDbContextFactory<CardanoDbSyncContext> cardanoDbSyncContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoDbSyncContextFactory = cardanoDbSyncContextFactory;
        _cardanoService = cardanoService;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraStakeDelegationEvent stakeDelegationEvent)
    {

        if (stakeDelegationEvent is not null &&
            stakeDelegationEvent.Context is not null &&
            stakeDelegationEvent.StakeDelegation is not null &&
            stakeDelegationEvent.StakeDelegation.Credential is not null &&
            stakeDelegationEvent.StakeDelegation.PoolHash is not null &&
            stakeDelegationEvent.Context.TxIdx is not null &&
            stakeDelegationEvent.Context.TxHash is not null)
        {
            ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)stakeDelegationEvent.Context.Slot!);

            if (epoch < _settings.FisoStartEpoch - 1 || epoch >= _settings.FisoEndEpoch) return;

            using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Transaction? transaction = await _dbContext.Transactions
                .Include(t => t.Block)
                .Where(t => t.Hash == stakeDelegationEvent.Context.TxHash)
                .FirstOrDefaultAsync();

            if (transaction is null) throw new NullReferenceException("Transaction does not exist!");

            if (transaction.Block.InvalidTransactions is not null &&
                transaction.Block.InvalidTransactions.Contains(transaction.Index)) return;

            string? stakeAddress = _cardanoService.GetStakeAddressFromEvent(stakeDelegationEvent);

            if (stakeAddress is null) return;

            string poolId = _cardanoService.PoolHashToBech32(stakeDelegationEvent.StakeDelegation.PoolHash);

            List<string> fisoPools = _settings.FisoPools
                .Where(fp => fp.JoinEpoch <= epoch)
                .Select(fp => fp.PoolId)
                .ToList();

            FisoDelegator? fisoDelegator = await _dbContext.FisoDelegators.Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch).FirstOrDefaultAsync();

            if (fisoDelegator is null && !fisoPools.Contains(poolId)) return;

            decimal delegatorStake = await GetStakeAddressLiveStakeByBlockAsync(stakeAddress, (int)stakeDelegationEvent.Context.BlockNumber!);
            FisoDelegation newFisoDelegation = new()
            {
                EpochNumber = epoch,
                TxHash = stakeDelegationEvent.Context.TxHash,
                Slot = (ulong)stakeDelegationEvent.Context.Slot,
                BlockNumber = (ulong)stakeDelegationEvent.Context.BlockNumber,
                StakeAddress = stakeAddress,
                FromPoolId = fisoDelegator?.PoolId,
                ToPoolId = poolId,
                LiveStake = (ulong)delegatorStake
            };

            // deduct from old fiso pool
            if (fisoDelegator is not null)
            {
                var fromFisoPool = await _dbContext.FisoPoolActiveStakes
                        .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
                        .FirstOrDefaultAsync();

                // deduct amount
                if (fromFisoPool is not null)
                {
                    fromFisoPool.StakeAmount -= (ulong)delegatorStake;
                    _dbContext.Update(fromFisoPool);
                    _dbContext.Remove(fisoDelegator);
                }
            }
            else
            {
                // add to new fiso pool
                if (fisoPools.Contains(poolId))
                {
                    var toFisoPool = await _dbContext.FisoPoolActiveStakes
                        .Where(fpas => fpas.PoolId == poolId && fpas.EpochNumber == epoch)
                        .FirstOrDefaultAsync();

                    if (toFisoPool is not null)
                    {
                        toFisoPool.StakeAmount += (ulong)delegatorStake;
                        _dbContext.Update(toFisoPool);
                    }
                }
            }

            await _dbContext.FisoDelegations.AddAsync(newFisoDelegation);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetStakeAddressLiveStakeByBlockAsync(string stakeAddress, int blockNumber)
    {
        using CardanoDbSyncContext _dbContext = await _cardanoDbSyncContextFactory.CreateDbContextAsync();
        long stakeId = await _dbContext.StakeAddresses
            .Where(sa => sa.View == stakeAddress)
            .Select(sa => sa.Id)
            .FirstOrDefaultAsync();

        decimal stakeAmount = await _dbContext.TxOuts
            .Include(o => o.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(o => o.Tx.Block.BlockNo <= blockNumber)
            .Where(o => stakeId == o.StakeAddressId! && !_dbContext.TxIns
                .Include(i => i.TxOut)
                .ThenInclude(to => to.Block)
                .Where(i => i.TxOut.Block.BlockNo <= blockNumber)
                .Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync(); ;

        int? epoch = await _dbContext.Blocks
            .Where(b => b.BlockNo == blockNumber)
            .Select(b => b.EpochNo)
            .FirstOrDefaultAsync();

        decimal rewardAmount = await _dbContext.Rewards
            .Where(r => stakeId == r.AddrId)
            .Where(r => r.SpendableEpoch <= epoch)
            .Select(r => r.Amount)
            .SumAsync();

        decimal withdrawalAmount = await _dbContext.Withdrawals
            .Include(w => w.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(w => w.Tx.Block.BlockNo <= blockNumber)
            .Where(w => stakeId == w.AddrId)
            .Select(w => w.Amount)
            .SumAsync();

        return stakeAmount + rewardAmount - withdrawalAmount;
    }
    public async Task RollbackAsync(Block rollbackBlock)
    {
        using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        List<FisoDelegation> fisoDelegations = await _dbContext.FisoDelegations
            .Where(fd => fd.BlockNumber == rollbackBlock.BlockNumber)
            .ToListAsync();

        List<string?> affectedPools = fisoDelegations.SelectMany(fd => new[] { fd.ToPoolId, fd.FromPoolId })
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        List<FisoPoolActiveStake> affectedFisoActiveStakes = await _dbContext.FisoPoolActiveStakes
            .Where(fpas => affectedPools.Contains(fpas.PoolId))
            .ToListAsync();

        foreach (FisoDelegation fisoDelegation in fisoDelegations)
        {
            FisoPoolActiveStake? affectedFromPool = affectedFisoActiveStakes.Where(fpas => fpas.PoolId == fisoDelegation.FromPoolId).FirstOrDefault();
            FisoPoolActiveStake? affectedToPool = affectedFisoActiveStakes.Where(fpas => fpas.PoolId == fisoDelegation.ToPoolId).FirstOrDefault();

            if (affectedFromPool is not null && fisoDelegation.FromPoolId is not null)
            {
                affectedFromPool.StakeAmount += fisoDelegation.LiveStake;

                FisoDelegator newFisoDelegator = new()
                {
                    StakeAddress = fisoDelegation.StakeAddress,
                    PoolId = fisoDelegation.FromPoolId,
                    StakeAmount = fisoDelegation.LiveStake,
                    Epoch = fisoDelegation.EpochNumber
                };

                await _dbContext.FisoDelegators.AddAsync(newFisoDelegator);
            }

            if (affectedToPool is not null)
            {
                affectedToPool.StakeAmount -= fisoDelegation.LiveStake;

                FisoDelegator? oldFisoDelegator = await _dbContext.FisoDelegators
                    .Where(fd => fd.StakeAddress == fisoDelegation.StakeAddress && fd.Epoch == fisoDelegation.EpochNumber)
                    .FirstOrDefaultAsync();

                if (oldFisoDelegator is not null)
                {
                    _dbContext.Remove(oldFisoDelegator);
                }
            }

            _dbContext.Remove(fisoDelegation);
        }

        _dbContext.UpdateRange(affectedFisoActiveStakes);
        await _dbContext.SaveChangesAsync();
    }
}