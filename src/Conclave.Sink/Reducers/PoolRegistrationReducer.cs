using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration, OuraVariant.PoolRetirement)]
public class PoolRegistrationReducer : OuraReducerBase
{
    private static int GET_METADATA_DELAY = 300;
    private static int MAX_RETRY_COUNT = 5;
    private readonly ILogger<PoolRegistrationReducer> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IHttpClientFactory _httpClientFactory;

    public PoolRegistrationReducer(
        ILogger<PoolRegistrationReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        OuraPoolRegistrationEvent? poolRegistrationEvent = ouraEvent as OuraPoolRegistrationEvent;

        if (poolRegistrationEvent is not null &&
            poolRegistrationEvent.PoolRegistration is not null &&
            poolRegistrationEvent.Context is not null &&
            poolRegistrationEvent.Context.Slot is not null &&
            poolRegistrationEvent.Context.BlockHash is not null)
        {
            Block? block = await _dbContext.Block
                .Where(block => block.BlockHash == poolRegistrationEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            JsonDocument? poolMetadataJSON = await GetJsonFromURLAsync(poolRegistrationEvent.PoolRegistration.PoolMetadata);
            string? metaDataHash = HashUtility.Blake2b256(poolMetadataJSON?.RootElement.ToString().ToBytes()).ToStringHex();

            if (metaDataHash == poolRegistrationEvent.PoolRegistration.PoolMetadataHash)
            {
                await _dbContext.PoolRegistration.AddAsync(new()
                {
                    Operator = poolRegistrationEvent.PoolRegistration.Operator,
                    VRFKeyHash = poolRegistrationEvent.PoolRegistration.VRFKeyHash,
                    Pledge = poolRegistrationEvent.PoolRegistration.Pledge,
                    Cost = poolRegistrationEvent.PoolRegistration.Cost,
                    Margin = poolRegistrationEvent.PoolRegistration.Margin,
                    RewardAccount = poolRegistrationEvent.PoolRegistration.RewardAccount,
                    PoolOwners = poolRegistrationEvent.PoolRegistration.PoolOwners,
                    Relays = poolRegistrationEvent.PoolRegistration.Relays,
                    PoolMetadata = poolMetadataJSON,
                    Block = block,
                    TxHash = poolRegistrationEvent.Context.TxHash,
                    PoolMetadataHash = poolRegistrationEvent.PoolRegistration.PoolMetadataHash
                });

                await _dbContext.SaveChangesAsync();
            }
        }
    }


    public async Task<JsonDocument?> GetJsonFromURLAsync(string? metaDataURL)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        int retries = 0;

        while (retries <= MAX_RETRY_COUNT)
        {
            try
            {
                return await client.GetFromJsonAsync<JsonDocument>(metaDataURL);
            }
            catch
            {
                await Task.Delay(GET_METADATA_DELAY);
                retries++;
            }
        }
        return null;
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<PoolRegistration> rollbackEntriesList = await _dbContext.PoolRegistration
            .Where(p => p.Block == rollbackBlock)
            .ToListAsync();

        if (rollbackEntriesList is not null &&
            rollbackEntriesList.Count is not 0)
            _dbContext.RemoveRange(rollbackEntriesList);

        await _dbContext.SaveChangesAsync();
    }
}