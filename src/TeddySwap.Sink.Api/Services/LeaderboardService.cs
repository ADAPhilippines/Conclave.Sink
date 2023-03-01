using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Data;

namespace TeddySwap.Sink.Api.Services;

public class LeaderboardService
{
    private readonly ILogger<LeaderboardService> _logger;
    private readonly TeddySwapSinkDbContext _dbContext;

    public LeaderboardService(
        ILogger<LeaderboardService> logger,
        TeddySwapSinkDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<LeaderboardHistoryResponse> FetchAllAsync(LeaderboardRequest? request)
    {

        var rewardQuery = await _dbContext.Orders
            .GroupBy(o => o.RewardAddress)
            .Select(g => new LeaderboardResponse
            {
                Address = g.Key,
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
                Address = g.Key,
                Total = 0,
                Deposit = 0,
                Redeem = 0,
                Swap = 0,
                Batch = g.Count()
            })
            .ToListAsync();

        var response = rewardQuery
            .Concat(batchQuery)
            .GroupBy(r => r.Address)
            .OrderByDescending(g => g.Sum(r => r.Total + r.Batch))
            .Select((g, rank) => new LeaderboardResponse
            {
                Address = g.Key,
                Rank = rank + 1,
                Total = g.Sum(r => r.Total + r.Batch),
                Deposit = g.Sum(r => r.Deposit),
                Redeem = g.Sum(r => r.Redeem),
                Swap = g.Sum(r => r.Swap),
                Batch = g.Sum(r => r.Batch)
            })
            .ToList();

        return new LeaderboardHistoryResponse()
        {
            Total = response.Count,
            LeaderboardHistory = response
        };
    }
}