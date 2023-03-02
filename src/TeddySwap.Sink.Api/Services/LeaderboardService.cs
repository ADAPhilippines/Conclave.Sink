using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Models;
using TeddySwap.Sink.Data;

namespace TeddySwap.Sink.Api.Services;

public class LeaderboardService
{
    private readonly ILogger<LeaderboardService> _logger;
    private readonly TeddySwapSinkDbContext _dbContext;
    private readonly TeddySwapITNRewardSettings _settings;

    public LeaderboardService(
        ILogger<LeaderboardService> logger,
        TeddySwapSinkDbContext dbContext,
        IOptions<TeddySwapITNRewardSettings> settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<PaginatedLeaderboardResponse> GetLeaderboardAsync(int offset, int limit)
    {

        var rewardQuery = await _dbContext.Orders
            .GroupBy(o => o.RewardAddress)
            .Select(g => new LeaderboardResponse
            {
                TestnetAddress = g.Key,
                Total = g.Count(o => o.OrderType != OrderType.Unknown),
                Deposit = g.Count(o => o.OrderType == OrderType.Deposit),
                Redeem = g.Count(o => o.OrderType == OrderType.Redeem),
                Swap = g.Count(o => o.OrderType == OrderType.Swap),
                Batch = 0
            })
            .ToListAsync();

        var batchQuery = await _dbContext.Orders
            .GroupBy(o => o.BatcherAddress)
            .Select(g => new LeaderboardResponse
            {
                TestnetAddress = g.Key,
                Total = 0,
                Deposit = 0,
                Redeem = 0,
                Swap = 0,
                Batch = g.Count()
            })
            .ToListAsync();

        var allEntries = rewardQuery
            .Concat(batchQuery)
            .GroupBy(r => r.TestnetAddress)
            .OrderByDescending(g => g.Sum(r => r.Total + r.Batch))
            .Select((g, rank) => new LeaderboardResponse
            {
                TestnetAddress = g.Key,
                Rank = rank + 1,
                Total = g.Sum(r => r.Total + r.Batch),
                Deposit = g.Sum(r => r.Deposit),
                Redeem = g.Sum(r => r.Redeem),
                Swap = g.Sum(r => r.Swap),
                Batch = g.Sum(r => r.Batch)
            })
            .ToList();

        decimal overallTotalAmount = allEntries.Sum(a => a.Total);

        var pagedEntries = allEntries
            .OrderByDescending(r => r.Total)
            .Skip(offset)
            .Take(limit)
            .Select((r, index) => new LeaderboardResponse
            {
                TestnetAddress = r.TestnetAddress,
                Rank = index + 1 + offset,
                Total = r.Total,
                Deposit = r.Deposit,
                Redeem = r.Redeem,
                Swap = r.Swap,
                Batch = r.Batch,
                BaseRewardPercentage = r.Total / overallTotalAmount,
                BaseReward = r.Total / overallTotalAmount * _settings.TotalReward
            })
            .ToList();

        int totalAmount = allEntries.Sum(r => r.Total);
        int totalCount = allEntries.Count;

        return new PaginatedLeaderboardResponse()
        {
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            Result = pagedEntries
        };
    }

    public async Task<LeaderboardResponse?> GetLeaderboardAddressAsync(string bech32Address)
    {
        var response = await GetLeaderboardAsync(0, int.MaxValue);
        var filteredResponse = response.Result
            .Where(l => l.TestnetAddress == bech32Address || l.MainnetAddress == bech32Address)
            .FirstOrDefault();

        return filteredResponse;
    }

    public async Task<PaginatedLeaderboardResponse> GetUserLeaderboardAsync(int offset, int limit)
    {
        var rewardQuery = await _dbContext.Orders
            .GroupBy(o => o.RewardAddress)
            .Select(g => new
            {
                Address = g.Key,
                Total = g.Count(o => o.OrderType != OrderType.Unknown),
                Deposit = g.Count(o => o.OrderType == OrderType.Deposit),
                Redeem = g.Count(o => o.OrderType == OrderType.Redeem),
                Swap = g.Count(o => o.OrderType == OrderType.Swap),
                Batch = 0
            })
            .ToListAsync();

        int totalCount = rewardQuery.Count;
        decimal overallTotalAmount = rewardQuery.Sum(r => r.Total);

        var response = rewardQuery
            .OrderByDescending(r => r.Total)
            .Select((r, i) => new LeaderboardResponse
            {
                TestnetAddress = r.Address,
                Total = r.Total,
                Deposit = r.Deposit,
                Redeem = r.Redeem,
                Swap = r.Swap,
                Batch = 0,
                Rank = i + 1,
                BaseRewardPercentage = r.Total / overallTotalAmount,
                BaseReward = r.Total / overallTotalAmount * _settings.UserReward
            })
            .Skip(offset)
            .Take(limit)
            .ToList();

        return new PaginatedLeaderboardResponse()
        {
            TotalAmount = (int)overallTotalAmount,
            TotalCount = totalCount,
            Result = response
        };
    }

    public async Task<LeaderboardResponse?> GetUserLeaderboardAddressAsync(string bech32Address)
    {
        var response = await GetUserLeaderboardAsync(0, int.MaxValue);
        var filteredResponse = response.Result
            .Where(l => l.TestnetAddress == bech32Address || l.MainnetAddress == bech32Address)
            .FirstOrDefault();

        return filteredResponse;
    }

    public async Task<PaginatedLeaderboardResponse> GetBatcherLeaderboardAsync(int offset, int limit)
    {
        var batchQuery = await _dbContext.Orders
          .GroupBy(o => o.BatcherAddress)
          .Select(g => new
          {
              Address = g.Key,
              TotalCount = g.Count(),
              BatchCount = g.Count()
          })
          .ToListAsync();

        decimal overallTotal = batchQuery.Sum(b => b.TotalCount);

        var response = batchQuery
            .OrderByDescending(b => b.TotalCount)
            .ThenBy(b => b.Address)
            .Select((b, rank) => new LeaderboardResponse
            {
                TestnetAddress = b.Address,
                Rank = rank + 1,
                Total = b.TotalCount,
                Deposit = 0,
                Redeem = 0,
                Swap = 0,
                Batch = b.BatchCount,
                BaseRewardPercentage = b.TotalCount / overallTotal,
                BaseReward = b.TotalCount / overallTotal * _settings.BatcherReward
            })
            .ToList();

        int totalCount = batchQuery.Count;
        var pagedEntries = response.Skip(offset).Take(limit).ToList();

        return new PaginatedLeaderboardResponse()
        {
            TotalAmount = (int)overallTotal,
            TotalCount = totalCount,
            Result = pagedEntries
        };
    }

    public async Task<LeaderboardResponse?> GetBatcherLeaderboardAddressAsync(string bech32Address)
    {
        var response = await GetBatcherLeaderboardAsync(0, int.MaxValue);
        var filteredResponse = response.Result
            .Where(l => l.TestnetAddress == bech32Address || l.MainnetAddress == bech32Address)
            .FirstOrDefault();

        return filteredResponse;
    }
}