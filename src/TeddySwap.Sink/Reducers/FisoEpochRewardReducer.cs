using System.Text.Json;
using CardanoSharp.Koios.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Block)]
public class FisoEpochRewardReducer : OuraReducerBase
{
    private readonly ILogger<FisoEpochRewardReducer> _logger;
    private readonly IDbContextFactory<TeddySwapFisoSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IPoolClient _poolClient;
    private readonly TeddySwapSinkSettings _settings;

    public FisoEpochRewardReducer(
        ILogger<FisoEpochRewardReducer> logger,
        IDbContextFactory<TeddySwapFisoSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        CardanoService cardanoService,
        IPoolClient poolClient
        )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _poolClient = poolClient;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraBlockEvent blockEvent)
    {
        if (blockEvent.Context is not null &&
            blockEvent.Context.BlockNumber is not null &&
            blockEvent.Context.Slot is not null)
        {
            using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
            Block? previousBlock = await _dbContext.Blocks
                .Where(b => b.Slot < blockEvent.Context.Slot)
                .OrderByDescending(b => b.Slot)
                .FirstOrDefaultAsync();

            if (previousBlock is null) return;

            ulong previousEpoch = _cardanoService.CalculateEpochBySlot(previousBlock.Slot);
            ulong calculationEpoch = _cardanoService.CalculateEpochBySlot((ulong)blockEvent.Context.Slot);

            // fiso reward has ended or not yet epoch boundary
            if (calculationEpoch >= _settings.FisoStartEpoch - 1 && calculationEpoch <= _settings.FisoEndEpoch && calculationEpoch > previousEpoch)
            {
                List<FisoPool> fisoPools = _settings.FisoPools
                    .Where(fp => fp.JoinEpoch <= calculationEpoch)
                    .ToList();

                // Calculate total stakes
                ulong totalStakes = 0;
                List<FisoPoolActiveStake> poolStakes = new();

                foreach (FisoPool fisoPool in fisoPools)
                {
                    var poolHistoryReq = await _poolClient.GetHistory(fisoPool.PoolId, calculationEpoch.ToString());
                    var poolHistory = poolHistoryReq.Content is not null && poolHistoryReq.Content.Length > 0 ? poolHistoryReq.Content[0] : null;

                    if (poolHistory is null || poolHistory.ActiveStake is null) continue;
                    totalStakes += ulong.Parse(poolHistory.ActiveStake);
                    poolStakes.Add(new FisoPoolActiveStake()
                    {
                        EpochNumber = calculationEpoch,
                        PoolId = fisoPool.PoolId,
                        StakeAmount = ulong.Parse(poolHistory.ActiveStake)
                    });
                }

                // Fetch all delegators
                List<FisoDelegator> delegators = new();

                foreach (FisoPool fisoPool in fisoPools)
                {
                    var poolDelegatorHistoryReq = await _poolClient.GetDelegatorsHistory(fisoPool.PoolId, calculationEpoch.ToString());
                    PoolDelegatorHistory[]? poolDelegatorHistory = poolDelegatorHistoryReq.Content;

                    if (poolDelegatorHistory is null || poolDelegatorHistory.Length < 1) continue;

                    delegators.AddRange(poolDelegatorHistory.Where(d => d.Amount != null && d.StakeAddress != null).Select(d => new FisoDelegator()
                    {
                        StakeAddress = d.StakeAddress!,
                        StakeAmount = ulong.Parse(d.Amount!),
                        PoolId = fisoPool.PoolId,
                        Epoch = calculationEpoch,
                    }));
                }

                // save snapshot of delegator stakes
                _dbContext.FisoDelegators.AddRange(delegators.DistinctBy(d => d.StakeAddress));
                _dbContext.FisoPoolActiveStakes.AddRange(poolStakes);

                // Only save if within the fiso duration
                if (calculationEpoch >= _settings.FisoStartEpoch && calculationEpoch <= _settings.FisoEndEpoch)
                {
                    decimal totalPoints = delegators.Sum(d => GetPoints(d.StakeAmount));

                    // Calculate fiso rewards
                    if (calculationEpoch >= _settings.FisoStartEpoch)
                    {
                        // Calculate share
                        List<FisoEpochReward> epochRewards = new();
                        foreach (FisoDelegator delegator in delegators)
                        {
                            decimal points = GetPoints(delegator.StakeAmount);
                            decimal sharePercentage = points / totalPoints;

                            epochRewards.Add(new()
                            {
                                EpochNumber = calculationEpoch,
                                StakeAddress = delegator.StakeAddress,
                                PoolId = delegator.PoolId,
                                StakeAmount = delegator.StakeAmount,
                                SharePercentage = sharePercentage,
                                ShareAmount = (ulong)(_settings.FisoRewardPerEpoch * sharePercentage)
                            });
                        }
                        _dbContext.FisoEpochRewards.AddRange(epochRewards);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    private static decimal GetAdaValue(ulong lovelace)
    {
        return lovelace / 1_000_000;
    }

    private static decimal GetPoints(ulong stakeAmount)
    {
        decimal adaStake = GetAdaValue(stakeAmount);
        decimal points = adaStake;

        if (points > 100000)
        {
            points = (decimal)Math.Pow((double)adaStake - 100000, 0.9) + 100000;
        }

        return points;
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}