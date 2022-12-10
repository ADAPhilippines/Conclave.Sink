using Blockfrost.Api;
using Conclave.Common.Models;
using Conclave.Common.Models.Entities;
using Conclave.Sink.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Api.Services;
public class AccountService
{
    private readonly IAccountService _accountService;
    private readonly ConclaveSinkDbContext _dbContext;
    private readonly ConclaveSettings _conclaveSettings;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IAccountService accountService, ConclaveSinkDbContext dbContext, IOptions<ConclaveSettings> conclaveSettings, ILogger<AccountService> logger)
    {
        _accountService = accountService;
        _dbContext = dbContext;
        _conclaveSettings = conclaveSettings.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<AccountEpochReward>> GetBaseEpochRewardsAsync(string stakeAddress, ulong end)
    {
        // Blockfrost implementation
        int page = 1;
        List<AccountEpochReward> rewards = new();
        while (true)
        {
            try
            {
                var rewardsPage = await _accountService.RewardsAsync(stakeAddress, page, 100, ESortOrder.Asc);

                if (rewardsPage is null) break;

                rewards.AddRange(rewardsPage.Select(rp => new AccountEpochReward
                {
                    Epoch = (ulong)rp.Epoch,
                    Amount = ulong.Parse(rp.Amount)
                }));

                if (rewardsPage.Count < 100 || rewards.LastOrDefault() is null ||
                    rewards.LastOrDefault()?.Epoch > end) break;

                page++;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting rewards for stake address {stakeAddress}", stakeAddress);
                break;
            }
        }

        return rewards ?? new List<AccountEpochReward>();
    }

    public async Task<IEnumerable<AccountEpochReward>> GetBaseEpochPendingRewardsAsync(string stakeAddress, ulong start, ulong end)
    {

        if (start > end) throw new ArgumentException("fromEpoch must be less than or equal to toEpoch");

        IEnumerable<AccountEpochReward> rewards = await GetBaseEpochRewardsAsync(stakeAddress, end);
        IQueryable<WithdrawalByStakeEpoch> withdrawals = _dbContext.WithdrawalByStakeEpoch
            .Where(w => w.StakeAddress == stakeAddress && w.Epoch <= end);

        List<AccountEpochReward> pendingRewards = new();

        ulong totalEpochReward = 0;
        for (ulong i = start; i <= end; i++)
        {
            totalEpochReward += rewards
                .Where(r => r.Epoch == i)
                .Select(r => r.Amount)
                .FirstOrDefault();

            ulong totalEpochWithdrawals = await withdrawals
                .Where(w => w.Epoch <= i)
                .OrderByDescending(w => w.Epoch)
                .Select(w => w.Amount)
                .FirstOrDefaultAsync();

            pendingRewards.Add(new AccountEpochReward
            {
                Epoch = i,
                Amount = totalEpochReward - totalEpochWithdrawals
            });
        }

        return pendingRewards;
    }

    public async Task<IEnumerable<AccountEpochStake>> GetBaseEpochStakes(string stakeAddress, ulong? start, ulong? end)
    {
        ulong currentEpoch = await _dbContext.Blocks.Select(b => b.Epoch).MaxAsync();

        start ??= 0;
        end = end is not null ? ulong.Min((ulong)end, currentEpoch) : currentEpoch;

        if (start > end) throw new ArgumentException("fromEpoch must be less than or equal to toEpoch");

        IEnumerable<AccountEpochReward> pendingRewards = await GetBaseEpochPendingRewardsAsync(stakeAddress, (ulong)start, (ulong)end);
        IQueryable<StakeDelegation> delegations = _dbContext.StakeDelegations
            .Include(sd => sd.Transaction).ThenInclude(t => t.Block)
            .Where(sd => sd.StakeAddress == stakeAddress && sd.Transaction.Block.Epoch <= end);

        var balances = _dbContext.CnclvByStakeEpoch
            .Join(_dbContext.BalanceByStakeEpoch
                .Where(b => b.StakeAddress == stakeAddress && b.Epoch <= end),
                c => c.Epoch,
                b => b.Epoch,
                (c, b) => new { c.Epoch, Lovelace = b.Balance, Conclave = c.Balance, })
            .OrderBy(b => b.Epoch);

        List<AccountEpochStake> epochStakes = new();

        for (ulong i = (ulong)start; i <= end; i++)
        {
            string? poolId = await delegations
                .Where(d => d.Transaction.Block.Epoch <= i)
                .OrderByDescending(d => d.Transaction.Block.Epoch)
                .Select(d => d.PoolId)
                .FirstOrDefaultAsync();

            if (poolId is null) continue;

            (ulong lovelace, ulong conclave) = balances
                .Where(b => b.Epoch <= i)
                .OrderByDescending(b => b.Epoch)
                .Select(b => new Tuple<ulong, ulong>(b.Lovelace, b.Conclave).ToValueTuple())
                .FirstOrDefault();

            ulong pendingReward = pendingRewards
                .Where(r => r.Epoch == i)
                .Select(r => r.Amount)
                .FirstOrDefault();


            epochStakes.Add(new AccountEpochStake
            {
                PoolId = poolId,
                Epoch = i,
                Lovelace = lovelace + pendingReward,
                Conclave = conclave,

            });
        }

        return epochStakes;
    }

    public async Task<IEnumerable<AccountEpochStake>> GetConclaveEpochStake(string stakeAddress, ulong? start, ulong? end)
    {

        ulong currentEpoch = await _dbContext.Blocks.Select(b => b.Epoch).MaxAsync();

        start ??= 0;
        end = end is not null ? ulong.Min((ulong)end, currentEpoch) : currentEpoch;

        if (start > end) throw new ArgumentException("fromEpoch must be less than or equal to toEpoch");

        IEnumerable<AccountEpochStake> baseStake = await GetBaseEpochStakes(stakeAddress, (ulong)start, (ulong)end);

        IEnumerable<ConclavePool>? pools = _conclaveSettings.Members
            .Where(m => m.Since >= start && m.Until <= end);

        IEnumerable<AccountEpochStake> conclaveEpochStake = new List<AccountEpochStake>();



        if (pools is not null)
        {
            for (ulong i = (ulong)start; i <= end; i++)
            {

            }
        }

        return conclaveEpochStake;
    }

    public ulong CalculateReward(ulong totalStakes, ulong delegatorStake, ulong totalReward)
    {
        decimal percentage = delegatorStake / totalStakes;
        return (ulong)(totalReward * percentage);
    }


    public async Task<ConclaveStake> GetTotalPoolStakes(string poolId, ulong epoch)
    {

        IEnumerable<string> poolDelegations = await GetPoolDelegatorsByEpochAsync(poolId, epoch);

        var balances = await _dbContext.CnclvByStakeEpoch
            .Join(_dbContext.BalanceByStakeEpoch
                .Where(b => b.Epoch <= epoch && poolDelegations.Contains(b.StakeAddress)),
                    c => c.Epoch,
                    b => b.Epoch,
                    (c, b) => new { c.Epoch, Lovelace = b.Balance, Conclave = c.Balance, b.StakeAddress })
            .GroupBy(j => j.StakeAddress)
            .Select(j => j.OrderByDescending(j => j.Epoch).First())
            .ToListAsync();

        ulong pendingRewards = 0;

        foreach (string stakeAddress in poolDelegations)
        {
            IEnumerable<AccountEpochReward> rewards = await GetBaseEpochPendingRewardsAsync(stakeAddress, epoch, epoch);
            pendingRewards += rewards.Aggregate(0ul, (sum, r) => sum + r.Amount);
        }

        var totalBalances = balances
            .Select(j => new ConclaveStake
            {
                Lovelace = j.Lovelace,
                Conclave = j.Conclave,
            });

        if (totalBalances is null) return new ConclaveStake { };

        ulong totalLovelace = totalBalances.Aggregate(0ul, (sum, b) => sum + b.Lovelace);
        ulong totalConclave = totalBalances.Aggregate(0ul, (sum, b) => sum + b.Conclave);

        return new ConclaveStake
        {
            Lovelace = totalLovelace + pendingRewards,
            Conclave = totalConclave,
        };
    }

    public async Task<IEnumerable<string>> GetPoolDelegatorsByEpochAsync(string poolId, ulong epoch)
    {
        IEnumerable<StakeDelegation>? delegations = await _dbContext.StakeDelegations
            .Include(sd => sd.Transaction).ThenInclude(t => t.Block)
            .Where(sd => sd.Transaction.Block.Epoch <= epoch)
            .GroupBy(sd => sd.StakeAddress)
            .Select(sd => sd.OrderByDescending(sd => sd.Transaction.Block.Slot).First())
            .ToListAsync();

        return delegations
            .Where(d => d.PoolId == poolId)
            .Select(d => d.StakeAddress);
    }

    public async Task<ConclaveStake> GetTotalConclaveStakes(ulong epoch)
    {
        Console.WriteLine(_conclaveSettings.ToString());
        IEnumerable<string> pools = _conclaveSettings.Members
            .Where(m => m.Since <= epoch && m.Until >= epoch)
            .Select(m => m.PoolId);

        ConclaveStake totalStakes = new();

        foreach (string pool in pools)
        {
            ConclaveStake poolStakes = await GetTotalPoolStakes(pool, epoch);
            totalStakes.Conclave += poolStakes.Conclave;
            totalStakes.Lovelace += poolStakes.Lovelace;
        }

        return totalStakes;
    }

    public async Task<IEnumerable<ConclaveEpochStakeRewards>> GetConclaveEpochStakeRewards(ulong epoch)
    {
        ulong totalCnclvReward = _conclaveSettings.Supply / _conclaveSettings.Duration * 1_000_000;
        ulong delegatorShare = (ulong)(totalCnclvReward * (_conclaveSettings.DelegatorShare / 100.0));
        ulong operatorShare = (ulong)(totalCnclvReward * (_conclaveSettings.OperatorShare / 100.0));

        List<ConclaveEpochStakeRewards> conclaveEpochStakeRewards = new();

        for (ulong i = _conclaveSettings.DistributionStart; i <= epoch; i++)
        {
            IEnumerable<string> pools = _conclaveSettings.Members
                .Where(m => m.Since >= i && m.Until <= i)
                .Select(m => m.PoolId);

            ConclaveStake totalStakes = await GetTotalConclaveStakes(_conclaveSettings.DistributionStart);

            foreach (string pool in pools)
            {
                IEnumerable<string> delegators = await GetPoolDelegatorsByEpochAsync(pool, i);
                IEnumerable<string>? poolOwners = await _dbContext.PoolRegistrations
                    .Include(p => p.Transaction).ThenInclude(t => t.Block)
                    .Where(p => p.PoolId == pool && p.Transaction.Block.Epoch <= i)
                    .OrderByDescending(p => p.Transaction.Block.Slot)
                    .Select(p => p.PoolOwners)
                    .FirstOrDefaultAsync();

                ulong poolOwnerShare = operatorShare / (ulong)pools.Count();
                ulong totalPoolOwnerStakes = 0;

                foreach (string delegator in delegators)
                {
                    AccountEpochStake? delegatorStake = (await GetBaseEpochStakes(delegator, i, i)).FirstOrDefault();
                    if (delegatorStake is null || delegatorStake.Lovelace <= 0) continue;

                    delegatorStake.Lovelace += conclaveEpochStakeRewards
                        .Where(r => r.StakeAddress == delegator && r.Epoch < i)
                        .Aggregate(0ul, (sum, r) => sum + r.Lovelace);

                    delegatorStake.Conclave += delegatorStake.Conclave + conclaveEpochStakeRewards
                            .Where(r => r.StakeAddress == delegator && r.Epoch < i)
                            .Aggregate(0ul, (sum, r) => sum + r.Conclave);

                    ulong delegatorCnclvReward = CalculateReward(totalStakes.Lovelace, delegatorStake.Lovelace, delegatorShare);

                    if (delegatorCnclvReward <= 0) continue;

                    conclaveEpochStakeRewards.Add(new ConclaveEpochStakeRewards
                    {
                        Epoch = i,
                        StakeAddress = delegator,
                        Lovelace = 0,
                        Conclave = delegatorCnclvReward,
                    });

                    if (poolOwners is null) continue;

                    if (poolOwners.Contains(delegator))
                    {
                        totalPoolOwnerStakes += delegatorStake.Lovelace;
                    }
                }

                if (poolOwners is null) continue;

                foreach (string poolOwner in poolOwners)
                {
                    AccountEpochStake? delegatorStake = (await GetBaseEpochStakes(poolOwner, i, i)).FirstOrDefault();
                    if (delegatorStake is null || delegatorStake.Lovelace <= 0) continue;

                    delegatorStake.Lovelace += conclaveEpochStakeRewards
                        .Where(r => r.StakeAddress == poolOwner && r.Epoch < i)
                        .Aggregate(0ul, (sum, r) => sum + r.Lovelace);

                    delegatorStake.Conclave += delegatorStake.Conclave + conclaveEpochStakeRewards
                        .Where(r => r.StakeAddress == poolOwner && r.Epoch < i)
                        .Aggregate(0ul, (sum, r) => sum + r.Conclave);


                    ulong poolOwnerCnclvReward = CalculateReward(totalPoolOwnerStakes, delegatorStake.Lovelace, poolOwnerShare);

                    ConclaveEpochStakeRewards? poolOwnerReward = conclaveEpochStakeRewards
                        .FirstOrDefault(r => r.StakeAddress == poolOwner && r.Epoch == i);

                    if (poolOwnerReward is null) continue;
                    poolOwnerReward.Conclave += poolOwnerCnclvReward;
                }
            }
        }
        return conclaveEpochStakeRewards;
    }
}