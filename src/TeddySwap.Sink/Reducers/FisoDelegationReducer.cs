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

            string stakeKeyHash = string.IsNullOrEmpty(stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash) ?
                stakeDelegationEvent.StakeDelegation.Credential.Scripthash :
                stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash;
            string stakeAddress = AddressUtility.GetRewardAddress(Convert.FromHexString(stakeKeyHash), _settings.NetworkType).ToString();
            string poolId = _cardanoService.PoolHashToBech32(stakeDelegationEvent.StakeDelegation.PoolHash);
            List<FisoPool> fisoPools = _settings.FisoPools
                .Where(fp => fp.JoinEpoch <= epoch)
                .ToList();

            var fisoDelegator = await _dbContext.FisoDelegators.Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch).FirstOrDefaultAsync();

            if (fisoDelegator is not null)
            {
                // if fisoDelegator left the pool
                if (fisoDelegator.PoolId != poolId)
                {
                    var fisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                        .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
                        .FirstOrDefaultAsync();

                    // deduct the stake amount
                    if (fisoPoolActiveStake is not null)
                    {
                        decimal stake = await GetStakeAddressLiveStakeByBlockAsync(stakeAddress, (int)stakeDelegationEvent.Context.BlockNumber!);
                        fisoPoolActiveStake.StakeAmount -= (ulong)stake;
                        _dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);
                    }

                    // if transferred to another fiso pool
                    if (fisoPools.Select(fp => fp.PoolId).Contains(poolId))
                    {
                        var newFisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                            .Where(fpas => fpas.PoolId == poolId && fpas.EpochNumber == epoch)
                            .FirstOrDefaultAsync();

                        if (newFisoPoolActiveStake is not null)
                        {
                            newFisoPoolActiveStake.StakeAmount += fisoDelegator.StakeAmount;
                            var newFisoDelegator = new FisoDelegator
                            {
                                // Copy over all properties except for the primary key
                                StakeAmount = fisoDelegator.StakeAmount,
                                PoolId = poolId,
                                StakeAddress = fisoDelegator.StakeAddress,
                                Epoch = fisoDelegator.Epoch

                            };
                            _dbContext.FisoPoolActiveStakes.Update(newFisoPoolActiveStake);
                            _dbContext.FisoDelegators.Remove(fisoDelegator);
                            await _dbContext.FisoDelegators.AddAsync(newFisoDelegator);
                        }
                    }
                    else
                    {
                        // else remove
                        _dbContext.FisoDelegators.Remove(fisoDelegator);
                    }

                }
            }
            else
            {
                // if new delegation
                if (fisoPools.Select(fp => fp.PoolId).Contains(poolId))
                {
                    var fisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                        .Where(fpas => fpas.PoolId == poolId && fpas.EpochNumber == epoch)
                        .FirstOrDefaultAsync();

                    decimal stake = await GetStakeAddressLiveStakeByBlockAsync(stakeAddress, (int)stakeDelegationEvent.Context.BlockNumber!);

                    // update stake amount
                    if (fisoPoolActiveStake is not null)
                    {
                        fisoPoolActiveStake.StakeAmount += (ulong)stake;
                        _dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);

                        // create new fiso delegator entry 
                        await _dbContext.FisoDelegators.AddAsync(new FisoDelegator()
                        {
                            StakeAddress = stakeAddress,
                            StakeAmount = (ulong)stake,
                            PoolId = poolId,
                            Epoch = epoch
                        });
                    }
                }
            }
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
    }
}