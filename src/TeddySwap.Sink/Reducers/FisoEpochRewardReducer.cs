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

            if (blockEvent.Context.BlockNumber == 511106)
            {
                Console.WriteLine("ere");
            }

            if (previousBlock is null) return;

            ulong previousEpoch = _cardanoService.CalculateEpochBySlot(previousBlock.Slot);
            ulong currentEpoch = _cardanoService.CalculateEpochBySlot((ulong)blockEvent.Context.Slot);

            // fiso reward has ended or not yet epoch boundary
            if (currentEpoch >= _settings.FisoStartEpoch - 1 && currentEpoch <= _settings.FisoEndEpoch && currentEpoch > previousEpoch)
            {

                List<FisoPool> fisoPools = _settings.FisoPools
                    .Where(fp => fp.JoinEpoch <= currentEpoch)
                    .ToList();

                // Calculate total stakes
                ulong totalStakes = 0;
                ulong calculationEpoch = currentEpoch - 1;
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

                // Calculate rewards if current epoch >= start epoch

                // Fetch all delegators
                List<Delegator> delegators = new();

                foreach (FisoPool fisoPool in fisoPools)
                {
                    var poolDelegatorHistoryReq = await _poolClient.GetDelegatorsHistory(fisoPool.PoolId, calculationEpoch.ToString());
                    PoolDelegatorHistory[]? poolDelegatorHistory = poolDelegatorHistoryReq.Content;

                    if (poolDelegatorHistory is null || poolDelegatorHistory.Length < 1) continue;

                    delegators.AddRange(poolDelegatorHistory.Where(d => ulong.Parse(d.Amount!) > 0).Select(d => new Delegator()
                    {
                        StakeAddress = d.StakeAddress!,
                        StakeAmount = ulong.Parse(d.Amount!),
                        PoolId = fisoPool.PoolId,
                        Epoch = calculationEpoch
                    }));
                }

                // save snapshot of delegator stakes
                _dbContext.FisoDelegators.AddRange(delegators);

                // Calculate fiso rewards
                if (currentEpoch >= _settings.FisoStartEpoch)
                {
                    // Calculate share
                    List<FisoEpochReward> epochRewards = delegators.Select(d => new FisoEpochReward()
                    {
                        EpochNumber = calculationEpoch,
                        StakeAddress = d.StakeAddress,
                        PoolId = d.PoolId,
                        StakeAmount = d.StakeAmount,
                        SharePercentage = d.StakeAmount / (decimal)totalStakes,
                        ShareAmount = (ulong)(_settings.FisoRewardPerEpoch * (d.StakeAmount / (decimal)totalStakes)),
                        ActiveBonus = false,
                        BonusAmount = 0
                    })
                    .ToList();

                    _dbContext.FisoEpochRewards.AddRange(epochRewards);
                }



                _dbContext.FisoPoolActiveStakes.AddRange(poolStakes);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}