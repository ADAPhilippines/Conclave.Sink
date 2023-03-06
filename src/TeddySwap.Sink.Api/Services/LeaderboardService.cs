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

    public async Task<PaginatedLeaderboardResponse> GetLeaderboardAsync(int offset, int limit, LeaderBoardType leaderboardType, List<string>? addresses)
    {

        var rewardQuery = _dbContext.Orders
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
            });

        var batchQuery = _dbContext.Orders
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
            });

        var entriesWithoutMainnetAddress = rewardQuery
            .Union(batchQuery)
            .GroupBy(r => r.TestnetAddress)
            .Select(g => new LeaderBoardResponse
            {
                TestnetAddress = g.Key,
                Total = g.Sum(r => leaderboardType == LeaderBoardType.All ? r.Total + r.Batch : leaderboardType == LeaderBoardType.Users ? r.Total : r.Batch),
                Deposit = g.Sum(r => r.Deposit),
                Redeem = g.Sum(r => r.Redeem),
                Swap = g.Sum(r => r.Swap),
                Batch = g.Sum(r => r.Batch),
            });

        var allEntriesWithMainnet = entriesWithoutMainnetAddress
            .GroupJoin(_dbContext.AddressVerifications,
                entry => entry.TestnetAddress,
                verification => verification.TestnetAddress,
                (entry, verifications) => new { Entry = entry, Verifications = verifications })
            .SelectMany(x => x.Verifications.DefaultIfEmpty(),
                (x, verification) => new LeaderBoardResponse
                {
                    TestnetAddress = x.Entry.TestnetAddress,
                    MainnetAddress = verification == null ? "" : verification.MainnetAddress,
                    Total = x.Entry.Total,
                    Deposit = x.Entry.Deposit,
                    Redeem = x.Entry.Redeem,
                    Swap = x.Entry.Swap,
                    Batch = x.Entry.Batch,
                });

        decimal overallTotalAmount = allEntriesWithMainnet.Sum(a => a.Total);
        int totalReward = leaderboardType switch
        {
            LeaderBoardType.Users => _settings.UserReward,
            LeaderBoardType.Badgers => _settings.BatcherReward,
            _ => _settings.TotalReward
        };

        var filteredEntriesQuery = allEntriesWithMainnet
            .Where(e => e.Total > 0)
            .OrderByDescending(e => e.Total)
            .AsEnumerable()
            .Select((entry, index) => new LeaderBoardResponse
            {
                TestnetAddress = entry.TestnetAddress,
                MainnetAddress = entry.MainnetAddress,
                Total = entry.Total,
                Deposit = entry.Deposit,
                Redeem = entry.Redeem,
                Swap = entry.Swap,
                Batch = entry.Batch,
                BaseRewardPercentage = entry.Total / overallTotalAmount,
                BaseReward = entry.Total / overallTotalAmount * totalReward,
                Rank = index + 1
            });

        if (addresses != null && addresses.Count > 0)
        {
            filteredEntriesQuery = filteredEntriesQuery.Where(r => addresses.Contains(r.TestnetAddress));
        }
        else
        {
            filteredEntriesQuery = filteredEntriesQuery.Skip(offset).Take(limit);
        }

        var filteredEntries = filteredEntriesQuery.ToList();


        foreach (LeaderBoardResponse response in filteredEntries)
        {

            if (!string.IsNullOrEmpty(response.MainnetAddress))
            {
                PaginatedAssetResponse res = await _assetService.GetAssetsAsync(
                    _settings.TbcPolicyId,
                    response.MainnetAddress, 0, 1, false);

                if (res.TotalCount > 0)
                {
                    response.TotalNft = res.TotalCount;
                    response.BonusReward = response.BaseReward * res.TotalCount * (decimal)0.05;
                }
            }
        }

        return new PaginatedLeaderboardResponse()
        {
            TotalAmount = allEntriesWithMainnet.Sum(r => r.Total),
            TotalCount = allEntriesWithMainnet.Where(t => t.Total > 0).ToList().Count,
            Result = filteredEntries
        };
    }

    public async Task<LeaderBoardResponse?> GetLeaderboardAddressAsync(string bech32Address, LeaderBoardType leaderBoardType)
    {

        var response = await GetLeaderboardAsync(0, int.MaxValue, leaderBoardType, new List<string>
        {
            bech32Address
        });

        return response.Result.FirstOrDefault();
    }

    public async Task<LeaderBoardResponse?> GetUserLeaderboardAddressesAsync(List<string> bech32Addresses, LeaderBoardType leaderBoardType)
    {

        var response = await GetLeaderboardAsync(0, int.MaxValue, leaderBoardType, bech32Addresses);
        var leaderboardResponses = response.Result;

        return response is not null && response.Result.Count > 0 ? new()
        {
            TestnetAddress = leaderboardResponses.FirstOrDefault()?.TestnetAddress ?? "",
            MainnetAddress = leaderboardResponses.FirstOrDefault()?.MainnetAddress ?? "",
            Rank = leaderboardResponses.Average(x => x.Rank),
            Total = leaderboardResponses.Sum(x => x.Total),
            Deposit = leaderboardResponses.Sum(x => x.Deposit),
            Redeem = leaderboardResponses.Sum(x => x.Redeem),
            Swap = leaderboardResponses.Sum(x => x.Swap),
            Batch = leaderboardResponses.Sum(x => x.Batch),
            BaseRewardPercentage = leaderboardResponses.Sum(x => x.BaseRewardPercentage),
            BaseReward = leaderboardResponses.Sum(x => x.BaseReward),
            BonusReward = leaderboardResponses.Sum(x => x.BonusReward),
            TotalNft = leaderboardResponses.Sum(x => x.TotalNft),
        } : null;
    }
}