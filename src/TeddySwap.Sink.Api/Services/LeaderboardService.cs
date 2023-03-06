using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Common.Enums;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Common.Models.Request;

namespace TeddySwap.Sink.Api.Services;

public class LeaderboardService
{
    private readonly ILogger<LeaderboardService> _logger;
    private readonly TeddySwapSinkDbContext _dbContext;
    private readonly TeddySwapITNRewardSettings _settings;
    private readonly AssetService _assetService;

    public LeaderboardService(
        ILogger<LeaderboardService> logger,
        TeddySwapSinkDbContext dbContext,
        AssetService assetService,
        IOptions<TeddySwapITNRewardSettings> settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _assetService = assetService;
        _settings = settings.Value;
    }

    public async Task<PaginatedLeaderboardResponse> GetLeaderboardAsync(int offset, int limit, LeaderBoardType leaderboardType)
    {
        var rewardQuery = await _dbContext.Orders
            .Where(o => !_dbContext.BlacklistedAddresses.Any(b => b.Address == o.UserAddress))
            .Where(o => o.Slot <= _settings.ItnEndSlot)
            .GroupBy(o => o.UserAddress)
            .Select(g => new LeaderBoardResponse
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
            .Where(b => b.BatcherAddress != null)
            .Where(o => !_dbContext.BlacklistedAddresses.Any(b => b.Address == o.BatcherAddress))
            .Where(o => o.Slot <= _settings.ItnEndSlot)
            .GroupBy(o => o.BatcherAddress)
            .Select(g => new LeaderBoardResponse
            {
                TestnetAddress = g.Key ?? "",
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
            .Select((g, rank) => new LeaderBoardResponse
            {
                TestnetAddress = g.Key,
                Rank = rank + 1,
                Total = g.Sum(r => leaderboardType switch
                {
                    LeaderBoardType.Users => r.Total,
                    LeaderBoardType.Badgers => r.Batch,
                    _ => r.Total + r.Batch
                }),
                Deposit = g.Sum(r => r.Deposit),
                Redeem = g.Sum(r => r.Redeem),
                Swap = g.Sum(r => r.Swap),
                Batch = g.Sum(r => r.Batch)
            })
            .ToList();

        decimal overallTotalAmount = allEntries.Sum(a => a.Total);
        int totalReward = leaderboardType switch
        {
            LeaderBoardType.Users => _settings.UserReward,
            LeaderBoardType.Badgers => _settings.BatcherReward,
            _ => _settings.TotalReward
        };

        var pagedEntries = allEntries
            .Where(r => r.Total > 0)
            .OrderByDescending(r => r.Total)
            .Skip(offset)
            .Take(limit)
            .Select((r, index) => new LeaderBoardResponse
            {
                TestnetAddress = r.TestnetAddress,
                Rank = index + 1 + offset,
                Total = r.Total,
                Deposit = r.Deposit,
                Redeem = r.Redeem,
                Swap = r.Swap,
                Batch = r.Batch,
                BaseRewardPercentage = r.Total / overallTotalAmount,
                BaseReward = r.Total / overallTotalAmount * totalReward
            })
            .ToList();

        foreach (LeaderBoardResponse response in pagedEntries)
        {
            AddressVerification? addressVerification = await _dbContext.AddressVerifications
                .Where(av => av.TestnetAddress == response.TestnetAddress)
                .FirstOrDefaultAsync();

            response.MainnetAddress = addressVerification is null ? "" : addressVerification.MainnetAddress;

            if (addressVerification is not null && !string.IsNullOrEmpty(addressVerification.MainnetAddress))
            {
                PaginatedAssetResponse res = await _assetService.GetAssetsAsync(new PaginatedAssetRequest()
                {
                    Limit = 1,
                    Offset = 0,
                    PolicyId = _settings.TbcPolicyId,
                    Address = addressVerification.MainnetAddress
                });

                if (res.TotalCount > 0)
                {
                    response.TotalNft = res.TotalCount;
                    response.BonusReward = response.BaseReward * res.TotalCount * (decimal)0.05;
                }
            }
        }

        int totalAmount = allEntries.Sum(r => r.Total);
        int totalCount = allEntries.Where(t => t.Total > 0).ToList().Count;

        return new PaginatedLeaderboardResponse()
        {
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            Result = pagedEntries
        };
    }

    public async Task<LeaderBoardResponse?> GetLeaderboardAddressAsync(string bech32Address, LeaderBoardType leaderBoardType)
    {
        var response = await GetLeaderboardAsync(0, int.MaxValue, leaderBoardType);
        var filteredResponse = response.Result
            .Where(l => l.TestnetAddress == bech32Address || l.MainnetAddress == bech32Address)
            .FirstOrDefault();

        return filteredResponse;
    }
}