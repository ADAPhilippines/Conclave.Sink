using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration, OuraVariant.PoolRetirement)]
public class PoolRegistrationReducer : OuraReducerBase
{
    private static int RETY_DURATION = 3000;
    private static int MAX_RETRIES = 5;
    private readonly ILogger<TxOutputReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private CardanoService _cardanoService;
    public PoolRegistrationReducer(
        ILogger<TxOutputReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        OuraPoolRegistrationEvent? poolRegistrationEvent = ouraEvent as OuraPoolRegistrationEvent;

        if (poolRegistrationEvent is not null &&
            poolRegistrationEvent.PoolRegistration is not null &&
            poolRegistrationEvent.PoolRegistration.PoolMetadata is not null &&
            poolRegistrationEvent.Context is not null &&
            poolRegistrationEvent.Context.Slot is not null &&
            poolRegistrationEvent.Context.BlockHash is not null)
        {
            Block? block = await _dbContext.Block
                .Where(block => block.BlockHash == poolRegistrationEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            string? poolMetadataJSON = await GetJsonFromURL(poolRegistrationEvent.PoolRegistration.PoolMetadata);

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
            });

            await _dbContext.SaveChangesAsync();
        }
    }


    public async Task<string?> GetJsonFromURL(string metaDataURL)
    {
        int retries = 0;

        while (retries <= MAX_RETRIES)
        {
            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(metaDataURL);
                string jsonString = await response.Content.ReadAsStringAsync();
                
                JsonDocument doc = JsonDocument.Parse(jsonString);
                return doc.RootElement.ToString();
            }
            catch
            {
                await Task.Delay(RETY_DURATION);
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