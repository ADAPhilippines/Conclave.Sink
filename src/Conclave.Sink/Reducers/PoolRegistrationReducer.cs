using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Common.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Conclave.Sink.Models.Oura;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration)]
public class PoolRegistrationReducer : OuraReducerBase
{
    private const int METADATA_REQUEST_RETRY_DELAY = 200;
    private const int METADATA_MAX_REQUEST_RETRY_COUNT = 5;
    private const int METADATA_REQUEST_TIMEOUT = 1000;
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
            poolRegistrationEvent.Context.BlockHash is not null &&
            poolRegistrationEvent.Context.TxHash is not null)
        {
            Transaction? transaction = await _dbContext.Transactions
                .Where(t => t.Hash == poolRegistrationEvent.Context.TxHash)
                .FirstOrDefaultAsync();
                
            if (transaction is not null &&
                transaction.Block.InvalidTransactions is not null &&
                transaction.Block.InvalidTransactions.Contains(transaction.Index))
                return;

            string? poolMetadataString = await GetJsonStringFromURLAsync(poolRegistrationEvent.PoolRegistration.PoolMetadata);
            string? metaDataHash = poolMetadataString is not null ? HashUtility.Blake2b256(poolMetadataString.ToBytes()).ToStringHex() : null;

            if (metaDataHash == poolRegistrationEvent.PoolRegistration.PoolMetadataHash &&
                transaction is not null)
            {
                JsonDocument? poolMetadataJSON = poolMetadataString is not null ? JsonDocument.Parse(poolMetadataString) : null;

                await _dbContext.PoolRegistrations.AddAsync(new()
                {
                    PoolId = _cardanoService.PoolHashToBech32(poolRegistrationEvent.PoolRegistration.Operator),
                    VrfKeyHash = poolRegistrationEvent.PoolRegistration.VRFKeyHash,
                    Pledge = poolRegistrationEvent.PoolRegistration.Pledge,
                    Cost = poolRegistrationEvent.PoolRegistration.Cost,
                    Margin = poolRegistrationEvent.PoolRegistration.Margin,
                    RewardAccount = _cardanoService.StakeHashToBech32(poolRegistrationEvent.PoolRegistration.RewardAccount),
                    PoolOwners = poolRegistrationEvent.PoolRegistration.PoolOwners.Select(po => _cardanoService.StakeHashToBech32(po)).ToList(),
                    Relays = poolRegistrationEvent.PoolRegistration.Relays,
                    PoolMetadataJSON = poolMetadataJSON,
                    PoolMetadataString = poolMetadataString,
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
        client.Timeout = TimeSpan.FromMilliseconds(METADATA_REQUEST_TIMEOUT);
        int retries = 0;

        while (retries <= METADATA_MAX_REQUEST_RETRY_COUNT)
        {
            try
            {
                return await client.GetStringAsync(metaDataURL);
            }
            catch
            {
                await Task.Delay(METADATA_REQUEST_RETRY_DELAY);
                retries++;
            }
        }

        return null;
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}