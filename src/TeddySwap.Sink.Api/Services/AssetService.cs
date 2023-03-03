using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.Enums;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Models;
using TeddySwap.Sink.Data;

namespace TeddySwap.Sink.Api.Services;

public class AssetService
{
    private readonly ILogger<AssetService> _logger;
    private readonly CardanoDbSyncContext _dbContext;
    private readonly TeddySwapITNRewardSettings _settings;

    public AssetService(
        ILogger<AssetService> logger,
        CardanoDbSyncContext dbContext,
        IOptions<TeddySwapITNRewardSettings> settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _settings = settings.Value;
    }

}