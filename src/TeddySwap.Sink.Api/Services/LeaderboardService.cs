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
        var result = await _dbContext.Orders
            .GroupBy(
            o => new { o.RewardAddress, o.BatcherAddress },
            o => o,
            (key, orders) => new
            {
                key.RewardAddress,
                key.BatcherAddress,
                SwapCount = orders.Count(o => o.OrderType == OrderType.Swap),
                DepositCount = orders.Count(o => o.OrderType == OrderType.Deposit),
                RedeemCount = orders.Count(o => o.OrderType == OrderType.Redeem),
                TotalCount = orders.Count()
            }
        )
        .ToListAsync();

        List<LeaderboardResponse> response = result
            .GroupBy(
                o => o.RewardAddress,
                o => o,
                (key, orders) => new
                {
                    key,
                    BatcherCount = orders.Select(o => o.BatcherAddress).Distinct().Count(),
                    SwapCount = orders.Sum(o => o.SwapCount),
                    DepositCount = orders.Sum(o => o.DepositCount),
                    RedeemCount = orders.Sum(o => o.RedeemCount),
                    TotalCount = orders.Sum(o => o.TotalCount)
                }
            )
            .OrderByDescending(o => o.TotalCount)
            .Select((o, i) => new
            {
                o.key,
                o.BatcherCount,
                o.SwapCount,
                o.DepositCount,
                o.RedeemCount,
                o.TotalCount,
                Rank = i + 1
            })
            .ToList()
            .Select(o => new LeaderboardResponse
            {
                Address = o.key,
                Swap = o.SwapCount,
                Deposit = o.DepositCount,
                Redeem = o.RedeemCount,
                Total = o.TotalCount,
                Batch = o.BatcherCount,
                Rank = o.Rank
            })
            .ToList();

        return new LeaderboardHistoryResponse()
        {
            Total = response.Count,
            LeaderboardHistory = response
        };
    }
}