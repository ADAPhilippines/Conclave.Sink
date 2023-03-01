namespace TeddySwap.Sink.Api.Services;

using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Data;

public class LeaderboardService
{
    private readonly ILogger<LeaderboardService> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;

    public LeaderboardService(
        ILogger<LeaderboardService> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

}