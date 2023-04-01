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

public class FisoRewardService
{
    private readonly ILogger<FisoRewardService> _logger;
    private readonly TeddySwapFisoSinkDbContext _dbContext;
    private readonly TeddySwapITNRewardSettings _settings;

    public FisoRewardService(
        ILogger<FisoRewardService> logger,
        TeddySwapFisoSinkDbContext dbContext,
        IOptions<TeddySwapITNRewardSettings> settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<FisoRewardBreakdownResponse> GetFisoRewardBreakdownAsync(string stakeAddress)
    {
        List<FisoRewardResponse> rewards = await _dbContext.FisoEpochRewards
            .Where(fer => fer.StakeAddress == stakeAddress)
            .OrderBy(fer => fer.EpochNumber)
            .Select(fer => new FisoRewardResponse()
            {
                PoolId = fer.PoolId,
                Epoch = fer.EpochNumber,
                ActiveStake = (ulong)fer.StakeAmount,
                BaseReward = fer.ShareAmount,
                BonusReward = 0
            })
            .ToListAsync();

        List<string> poolsWithBonus = new();

        foreach (string poolId in rewards.Select(r => r.PoolId).Distinct())
        {
            FisoBonusDelegation? bonusPoolDelegation = await _dbContext.FisoBonusDelegations
                .Where(fbd => fbd.StakeAddress == stakeAddress && fbd.PoolId == poolId)
                .FirstOrDefaultAsync();

            if (bonusPoolDelegation is null) continue;
            if (rewards.Where(r => r.PoolId == poolId).Count() >= 6) poolsWithBonus.Add(poolId);
        }

        rewards.ForEach(r => { if (poolsWithBonus.Contains(r.PoolId)) r.BonusReward = (ulong)(r.BaseReward * 0.25); });

        return new()
        {
            FisoRewards = rewards,
            StakeAddress = stakeAddress,
            TotalBaseReward = (ulong)rewards.Sum(r => (long)r.BaseReward),
            TotalBonusReward = (ulong)rewards.Sum(r => (long)r.BonusReward)
        };
    }
}