using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration)]
public class PoolRegistrationReducer : OuraReducerBase
{
    private const int GET_METADATA_DELAY = 200;
    private const int MAX_RETRY_COUNT = 5;
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
            poolRegistrationEvent.Context.BlockHash is not null)
        {
            Block? block = await _dbContext.Block
                .Where(b => b.BlockHash == poolRegistrationEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            Transaction? transaction = await _dbContext.Transaction
                .Where(t => t.Hash == poolRegistrationEvent.Context.TxHash)
                .FirstOrDefaultAsync();

            string? poolMetadataString = await GetJsonStringFromURLAsync(poolRegistrationEvent.PoolRegistration.PoolMetadata);
            string? metaDataHash = poolMetadataString is null ? null : HashUtility.Blake2b256(poolMetadataString.ToBytes()).ToStringHex();
            List<string> poolOwnersbech32 = poolRegistrationEvent.PoolRegistration.PoolOwners.Select(po => _cardanoService.RewardAddressHashToBech32(po)).ToList();

            if (metaDataHash == poolRegistrationEvent.PoolRegistration.PoolMetadataHash)
            {
                JsonDocument? poolMetadataJSON = poolMetadataString is null ? null : JsonDocument.Parse(poolMetadataString);

                await _dbContext.PoolRegistration.AddAsync(new()
                {
                    PoolId = poolRegistrationEvent.PoolRegistration.Operator,
                    PoolIdBech32 = _cardanoService.PoolHashToBech32(poolRegistrationEvent.PoolRegistration.Operator),
                    VRFKeyHash = poolRegistrationEvent.PoolRegistration.VRFKeyHash,
                    Pledge = poolRegistrationEvent.PoolRegistration.Pledge,
                    Cost = poolRegistrationEvent.PoolRegistration.Cost,
                    Margin = poolRegistrationEvent.PoolRegistration.Margin,
                    RewardAccount = _cardanoService.RewardAddressHashToBech32(poolRegistrationEvent.PoolRegistration.RewardAccount),
                    PoolOwners = poolOwnersbech32,
                    Relays = poolRegistrationEvent.PoolRegistration.Relays,
                    PoolMetadataJSON = poolMetadataJSON,
                    PoolMetadataString = poolMetadataString,
                    Block = block,
                    PoolMetadataHash = metaDataHash,
                    TxHash = poolRegistrationEvent.Context.TxHash,
                    Transaction = transaction
                });

                await _dbContext.SaveChangesAsync();
            }
        }
    }


    public async Task<string?> GetJsonStringFromURLAsync(string? metaDataURL)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        int retries = 0;

        while (retries <= MAX_RETRY_COUNT)
        {
            try
            {
                return await client.GetStringAsync(metaDataURL);
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
        await Task.CompletedTask;
    }
}