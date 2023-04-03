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
            .Where(fer => fer.StakeAddress == stakeAddress && fer.EpochNumber <= _settings.FisoEndEpoch)
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


        ulong maxEpoch = await _dbContext.FisoPoolActiveStakes
            .Select(fpas => fpas.EpochNumber)
            .MaxAsync();

        foreach (string poolId in rewards.Select(r => r.PoolId).Distinct())
        {
            var bonusPoolDelegations = await _dbContext.FisoBonusDelegations
                .Where(fbd => fbd.StakeAddress == stakeAddress && fbd.PoolId == poolId)
                .OrderBy(r => r.Slot)
                .ToListAsync();

            var bonusPoolDelegation = bonusPoolDelegations
                .Where(bpd => rewards.Where(r => r.PoolId == bpd.PoolId && r.Epoch == bpd.EpochNumber + 1).FirstOrDefault() != null)
                .OrderBy(bpd => bpd.Slot)
                .FirstOrDefault();

            if (bonusPoolDelegation is null) continue;

            var filteredRewards = rewards.Where(r => r.PoolId == poolId && r.Epoch > bonusPoolDelegation.EpochNumber).ToList();
            var epochWithBonusCount = filteredRewards.Count + (int)_settings.FisoEndEpoch - (int)maxEpoch;

            if (epochWithBonusCount >= 6) filteredRewards.ForEach(r => { r.BonusReward = (ulong)(r.BaseReward * 0.25); });
        }

        return new()
        {
            FisoRewards = rewards,
            StakeAddress = stakeAddress,
            TotalBaseReward = (ulong)rewards.Sum(r => (long)r.BaseReward),
            TotalBonusReward = (ulong)rewards.Sum(r => (long)r.BonusReward)
        };
    }


    // private async  HasBonus(List<FisoRewardResponse> rewards, ulong startEpoch, ulong maxEpoch)
    // {
    //     var filteredRewards = rewards.OrderBy(r => r.Epoch).Where(r => r.Epoch >= startEpoch);

    //     if (filteredRewards.Count() == 1)

    //     for (int i = 0; i < filteredRewards.Count(); i++)
    //     {
    //         if (filteredRewards[0])
    //     }

    //     return true;
    // }
}