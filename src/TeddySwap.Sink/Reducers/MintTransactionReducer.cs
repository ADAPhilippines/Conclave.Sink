
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class MintTransactionReducer : OuraReducerBase
{
    private readonly ILogger<MintTransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapNftSinkDbContext> _dbContextFactory;
    private readonly TeddySwapSinkSettings _settings;
    private readonly ByteArrayService _byteArrayService;

    public MintTransactionReducer(
        ILogger<MintTransactionReducer> logger,
        IDbContextFactory<TeddySwapNftSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        ByteArrayService byteArrayService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _byteArrayService = byteArrayService;
    }

    public async Task ReduceAsync(OuraTransaction transaction)
    {

        if (transaction is not null &&
            transaction.Context is not null &&
            transaction.Fee is not null &&
            transaction.Metadata is not null)
        {

            using TeddySwapNftSinkDbContext? _dbContext = await _dbContextFactory.CreateDbContextAsync();

            if (_dbContext is null) return;

            Transaction? existingTransaction = await _dbContext.Transactions
                .Where(t => t.Hash == transaction.Hash)
                .FirstOrDefaultAsync();

            if (existingTransaction is null) return;

            foreach (Metadatum metadatum in transaction.Metadata)
            {
                if (metadatum.Content is null) continue;
                foreach (string policyId in _settings.NftPolicyIds)
                {
                    // Deserialize Content to a JsonElement
                    JsonElement? jsonElement = JsonSerializer.Deserialize<JsonElement>(metadatum.Content.ToString()!);

                    if (jsonElement.Value.TryGetProperty(policyId, out JsonElement policyMetadata))
                    {
                        foreach (var asset in policyMetadata.EnumerateObject())
                        {
                            await _dbContext.MintTransactions.AddAsync(new()
                            {
                                PolicyId = policyId,
                                TokenName = Convert.ToHexString(Encoding.ASCII.GetBytes(asset.Name)).ToLower(),
                                AsciiTokenName = asset.Name,
                                Transaction = existingTransaction
                            });
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block _) => await Task.CompletedTask;
}