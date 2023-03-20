
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
    private readonly MetadataService _metadataService;

    public MintTransactionReducer(
        ILogger<MintTransactionReducer> logger,
        IDbContextFactory<TeddySwapNftSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        ByteArrayService byteArrayService,
        MetadataService metadataService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _byteArrayService = byteArrayService;
        _metadataService = metadataService;
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

            List<AssetClass> assets = _metadataService.FindAssets(transaction, _settings.NftPolicyIds.ToList());

            foreach (AssetClass asset in assets)
            {
                await _dbContext.MintTransactions.AddAsync(new()
                {
                    PolicyId = asset.PolicyId.ToLower(),
                    TokenName = asset.Name,
                    AsciiTokenName = asset.AsciiName ?? "",
                    Metadata = asset.Metadata,
                    Transaction = existingTransaction
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {

        if (rollbackBlock is not null)
        {
            using TeddySwapNftSinkDbContext? _dbContext = await _dbContextFactory.CreateDbContextAsync();
            List<Transaction>? transactions = await _dbContext.Transactions
                .Where(t => t.Block == rollbackBlock)
                .ToListAsync();

            if (transactions is not null)
            {
                List<MintTransaction>? mintTransactions = await _dbContext.MintTransactions
                    .Include(mtx => mtx.Transaction)
                    .Where(mtx => transactions.Contains(mtx.Transaction))
                    .ToListAsync();

                if (mintTransactions is not null)
                {
                    _dbContext.RemoveRange(mintTransactions);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}