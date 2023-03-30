using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.CardanoDbSync;
using TeddySwap.Common.Enums;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Models;
using TeddySwap.Sink.Data;

namespace TeddySwap.Sink.Api.Services;

public class StakeService
{
    private readonly ILogger<StakeService> _logger;
    private readonly CardanoDbSyncContext _dbContext;
    private readonly TeddySwapITNRewardSettings _settings;

    public StakeService(
        ILogger<StakeService> logger,
        CardanoDbSyncContext dbContext,
        IOptions<TeddySwapITNRewardSettings> settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<decimal> GetPoolLiveStakeAsync(string poolId)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        IQueryable<long> stakeIds = _dbContext.Delegations
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations.Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations.Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.AddrId);

        decimal stakeAmount = await _dbContext.TxOuts
            .Where(o => stakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns.Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        int? maxEpoch = await _dbContext.Blocks.Select(b => b.EpochNo).MaxAsync();

        decimal rewardAmount = await _dbContext.Rewards
            .Where(r => stakeIds.Contains(r.AddrId))
            .Where(r => r.SpendableEpoch <= maxEpoch)
            .Select(r => r.Amount)
            .SumAsync();

        decimal withdrawalAmount = await _dbContext.Withdrawals
            .Where(w => stakeIds.Contains(w.AddrId))
            .Select(w => w.Amount)
            .SumAsync();

        return stakeAmount + rewardAmount - withdrawalAmount;
    }

    public async Task<decimal> GetPoolBaseLiveStakeAsync(string poolId)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> stakeIds = await GetPoolDelegatorStakeIdsAsync(poolId);

        decimal stakeAmount = await _dbContext.TxOuts
            .Where(o => stakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns.Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        return stakeAmount;
    }

    public async Task<decimal> GetStakeAddressLiveStakeAsync(string stakeAddress)
    {
        long stakeId = await _dbContext.StakeAddresses
            .Where(sa => sa.View == stakeAddress)
            .Select(sa => sa.Id)
            .FirstOrDefaultAsync();

        decimal stakeAmount = await _dbContext.TxOuts
            .Where(o => stakeId == o.StakeAddressId! && !_dbContext.TxIns.Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        int? maxEpoch = await _dbContext.Blocks.Select(b => b.EpochNo).MaxAsync();

        decimal rewardAmount = await _dbContext.Rewards
            .Where(r => stakeId == r.AddrId)
            .Where(r => r.SpendableEpoch <= maxEpoch)
            .Select(r => r.Amount)
            .SumAsync();

        decimal withdrawalAmount = await _dbContext.Withdrawals
            .Where(w => stakeId == w.AddrId)
            .Select(w => w.Amount)
            .SumAsync();

        return stakeAmount + rewardAmount - withdrawalAmount;
    }

    public async Task<decimal> GetPoolLiveStakeByBlockAsync(string poolId, int blockNumber)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> stakeIds = await _dbContext.Delegations
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations
                .Include(d2 => d2.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(d2 => d2.Tx.Block.BlockNo <= blockNumber)
                .Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations
                .Include(sd => sd.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(sd => sd.Tx.Block.BlockNo <= blockNumber)
                .Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.AddrId)
            .ToListAsync();

        decimal stakeAmount = await _dbContext.TxOuts
            .Include(o => o.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(o => o.Tx.Block.BlockNo <= blockNumber)
            .Where(o => stakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns
                .Include(i => i.TxOut)
                .ThenInclude(to => to.Block)
                .Where(i => i.TxOut.Block.BlockNo <= blockNumber)
                .Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        int? epoch = await _dbContext.Blocks
            .Where(b => b.BlockNo == blockNumber)
            .Select(b => b.EpochNo)
            .FirstOrDefaultAsync();

        decimal rewardAmount = await _dbContext.Rewards
            .Where(r => stakeIds.Contains(r.AddrId))
            .Where(r => r.SpendableEpoch <= epoch)
            .Select(r => r.Amount)
            .SumAsync();

        decimal withdrawalAmount = await _dbContext.Withdrawals
            .Include(w => w.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(w => w.Tx.Block.BlockNo <= blockNumber)
            .Where(w => stakeIds.Contains(w.AddrId))
            .Select(w => w.Amount)
            .SumAsync();

        return stakeAmount + rewardAmount - withdrawalAmount;
    }

    public async Task<Dictionary<int, decimal>?> GetPoolLiveStakeTotalsByBlockAsync(string poolId, List<int> blockNumbers)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        var query = _dbContext.Blocks
            .Where(b => b.BlockNo != null)
            .Where(b => blockNumbers.Contains((int)b.BlockNo!))
            .Select(b => new
            {
                BlockNumber = b.BlockNo,
                StakeIds = _dbContext.Delegations
                    .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
                    .Where(x => x.ph.HashRaw == poolBytes)
                    .Where(x => !_dbContext.Delegations
                        .Include(d2 => d2.Tx)
                        .ThenInclude(tx => tx.Block)
                        .Where(d2 => d2.Tx.Block.BlockNo <= b.BlockNo)
                        .Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
                    .Where(x => !_dbContext.StakeDeregistrations
                        .Include(sd => sd.Tx)
                        .ThenInclude(tx => tx.Block)
                        .Where(sd => sd.Tx.Block.BlockNo <= b.BlockNo)
                        .Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
                    .Select(x => x.d1.AddrId)
                    .ToList()
            })
            .AsEnumerable()
            .Select(async x => new
            {
                x.BlockNumber,
                StakeAmount = await _dbContext.TxOuts
                    .Include(o => o.Tx)
                    .ThenInclude(tx => tx.Block)
                    .Where(o => o.Tx.Block.BlockNo == x.BlockNumber)
                    .Where(o => x.StakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns
                        .Include(i => i.TxOut)
                        .ThenInclude(to => to.Block)
                        .Where(i => i.TxOut.Block.BlockNo <= x.BlockNumber)
                        .Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
                    .Select(to => to.Value)
                    .SumAsync(),
                RewardAmount = await _dbContext.Rewards
                    .Where(r => x.StakeIds.Contains(r.AddrId))
                    .Where(r => r.SpendableEpoch <= x.BlockNumber)
                    .Select(r => r.Amount)
                    .SumAsync(),
                WithdrawalAmount = await _dbContext.Withdrawals
                    .Include(w => w.Tx)
                    .ThenInclude(tx => tx.Block)
                    .Where(w => w.Tx.Block.BlockNo == x.BlockNumber)
                    .Where(w => x.StakeIds.Contains(w.AddrId))
                    .Select(w => w.Amount)
                    .SumAsync()
            })
            .ToList();

        var results = await Task.WhenAll(query);

        return results.ToDictionary(x => (int)x.BlockNumber!, x => x.StakeAmount + x.RewardAmount - x.WithdrawalAmount);
    }

    public async Task<decimal> GetPoolBaseLiveStakeByBlockAsync(string poolId, int blockNumber)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> stakeIds = await GetPoolDelegatorStakeIdsByBlockAsync(poolId, blockNumber);

        decimal stakeAmount = await _dbContext.TxOuts
            .Include(o => o.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(o => o.Tx.Block.BlockNo <= blockNumber)
            .Where(o => stakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns
                .Include(i => i.TxOut)
                .ThenInclude(to => to.Block)
                .Where(i => i.TxOut.Block.BlockNo <= blockNumber)
                .Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        return stakeAmount;
    }


    public async Task<decimal> GetPoolLiveStakeDeltaByBlockAsync(string poolId, int startBlockNumber, int endBlockNumber, decimal offsetBaseStake)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> stakeIds = await GetPoolDelegatorStakeIdsByBlockAsync(poolId, endBlockNumber);

        decimal stakeAmount = await _dbContext.TxOuts
            .Include(o => o.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(o => o.Tx.Block.BlockNo <= endBlockNumber && o.Tx.Block.BlockNo >= startBlockNumber)
            .Where(o => stakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns
                .Include(i => i.TxOut)
                .ThenInclude(to => to.Block)
                .Where(i => i.TxOut.Block.BlockNo <= endBlockNumber && o.Tx.Block.BlockNo >= startBlockNumber)
                .Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        List<long> deregistrationStakeIds = await GetPoolDeregistrationStakeIdsByBlockAsync(poolId, endBlockNumber);

        decimal deregisteredAmounts = await _dbContext.TxOuts
            .Include(o => o.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(o => o.Tx.Block.BlockNo <= endBlockNumber && o.Tx.Block.BlockNo >= startBlockNumber)
            .Where(o => deregistrationStakeIds.Contains((long)o.StakeAddressId!) && !_dbContext.TxIns
                .Include(i => i.TxOut)
                .ThenInclude(to => to.Block)
                .Where(i => i.TxOut.Block.BlockNo <= endBlockNumber && o.Tx.Block.BlockNo >= startBlockNumber)
                .Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .Select(to => to.Value)
            .SumAsync();

        int? epoch = await _dbContext.Blocks
            .Where(b => b.BlockNo == endBlockNumber)
            .Select(b => b.EpochNo)
            .FirstOrDefaultAsync();

        decimal rewardAmount = await GetTotalRewardsAsync(stakeIds, epoch ?? 0);
        decimal withdrawalAmount = await GetTotalWithdrawalsByBlockAsync(stakeIds, endBlockNumber);

        return stakeAmount - deregisteredAmounts + offsetBaseStake + rewardAmount - withdrawalAmount;
    }

    public async Task<decimal> GetStakeAddressLiveStakeByBlockAsync(string stakeAddress, int blockNumber)
    {
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

    public async Task<List<string>> GetPoolDelegatorsAsync(string poolId)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<string> stakeAddresses = await _dbContext.Delegations
            .Include(d => d.Addr)
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations.Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations.Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.Addr.View)
            .ToListAsync();

        return stakeAddresses;
    }

    public async Task<List<long>> GetPoolDelegatorStakeIdsAsync(string poolId)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> stakeIds = await _dbContext.Delegations
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations.Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations.Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.AddrId)
            .ToListAsync();

        return stakeIds;
    }

    public async Task<List<long>> GetPoolDelegatorStakeIdsByBlockAsync(string poolId, int blockNumber)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> stakeIds = await _dbContext.Delegations
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations
                .Include(d2 => d2.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(d2 => d2.Tx.Block.BlockNo <= blockNumber)
                .Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations
                .Include(sd => sd.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(sd => sd.Tx.Block.BlockNo <= blockNumber)
                .Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.AddrId)
            .ToListAsync();

        return stakeIds;
    }

    public async Task<List<long>> GetPoolDeregistrationStakeIdsByBlockAsync(string poolId, int blockNumber)
    {
        byte[] poolBytes = Convert.FromHexString(poolId);

        List<long> previousStakeIds = await _dbContext.Delegations
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations
                .Include(d2 => d2.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(d2 => d2.Tx.Block.BlockNo <= blockNumber - 1)
                .Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations
                .Include(sd => sd.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(sd => sd.Tx.Block.BlockNo <= blockNumber - 1)
                .Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.AddrId)
            .ToListAsync();

        List<long> currentStakeIds = await _dbContext.Delegations
            .Join(_dbContext.PoolHashes, d1 => d1.PoolHashId, ph => ph.Id, (d1, ph) => new { d1, ph })
            .Where(x => x.ph.HashRaw == poolBytes)
            .Where(x => !_dbContext.Delegations
                .Include(d2 => d2.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(d2 => d2.Tx.Block.BlockNo <= blockNumber)
                .Any(d2 => d2.AddrId == x.d1.AddrId && d2.TxId > x.d1.TxId))
            .Where(x => !_dbContext.StakeDeregistrations
                .Include(sd => sd.Tx)
                .ThenInclude(tx => tx.Block)
                .Where(sd => sd.Tx.Block.BlockNo <= blockNumber)
                .Any(sd => sd.AddrId == x.d1.AddrId && sd.TxId > x.d1.TxId))
            .Select(x => x.d1.AddrId)
            .ToListAsync();

        return previousStakeIds.Where(psi => currentStakeIds.Contains(psi)).ToList();
    }

    public async Task<decimal> GetTotalWithdrawalsAsync(List<long> stakeIds)
    {
        decimal withdrawalAmount = await _dbContext.Withdrawals
            .Where(w => stakeIds.Contains(w.AddrId))
            .Select(w => w.Amount)
            .SumAsync();

        return withdrawalAmount;
    }

    public async Task<decimal> GetTotalWithdrawalsByBlockAsync(List<long> stakeIds, long blockNumber)
    {
        decimal withdrawalAmount = await _dbContext.Withdrawals
            .Include(w => w.Tx)
            .ThenInclude(tx => tx.Block)
            .Where(w => w.Tx.Block.BlockNo <= blockNumber)
            .Where(w => stakeIds.Contains(w.AddrId))
            .Select(w => w.Amount)
            .SumAsync();

        return withdrawalAmount;
    }

    public async Task<decimal> GetTotalRewardsAsync(List<long> stakeIds, long epoch)
    {
        decimal rewardAmount = await _dbContext.Rewards
            .Where(r => stakeIds.Contains(r.AddrId))
            .Where(r => r.SpendableEpoch <= epoch)
            .Select(r => r.Amount)
            .SumAsync();

        return rewardAmount;
    }
}