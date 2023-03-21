using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Asset)]
public class NftOwnerReducer : OuraReducerBase
{
    private readonly ILogger<NftOwnerReducer> _logger;
    private readonly IDbContextFactory<TeddySwapNftSinkDbContext> _dbContextFactory;
    private readonly TeddySwapSinkSettings _settings;
    private readonly MetadataService _metadataService;

    public NftOwnerReducer(
        ILogger<NftOwnerReducer> logger,
        IDbContextFactory<TeddySwapNftSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        MetadataService metadataService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _metadataService = metadataService;
    }

    public async Task ReduceAsync(OuraAssetEvent asset)
    {
        if (asset is not null &&
            asset.Address is not null &&
            asset.PolicyId is not null &&
            asset.TokenName is not null)
        {
            // skip invalid transactions
            if (asset.Context is not null &&
                asset.Context.InvalidTransactions is not null &&
                asset.Context.InvalidTransactions.ToList().Contains((ulong)asset.Context.TxIdx!)) return;

            if (_settings.NftPolicyIds.Contains(asset.PolicyId))
            {
                using TeddySwapNftSinkDbContext? _dbContext = await _dbContextFactory.CreateDbContextAsync();
                NftOwner? owner = await _dbContext.NftOwners
                    .Where(n => n.PolicyId == asset.PolicyId.ToLower() && n.TokenName == asset.TokenName.ToLower())
                    .FirstOrDefaultAsync();

                if (owner is null)
                {
                    await _dbContext.NftOwners.AddAsync(new()
                    {
                        Address = asset.Address,
                        PolicyId = asset.PolicyId.ToLower(),
                        TokenName = asset.TokenName.ToLower(),
                    });
                }
                else
                {
                    if (owner.Address != asset.Address)
                    {
                        owner.Address = asset.Address;
                        _dbContext.NftOwners.Update(owner);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        if (rollbackBlock is not null)
        {
            using TeddySwapNftSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
            List<Transaction>? transactions = await _dbContext.Transactions
                 .Include(tx => tx.Inputs)
                 .ThenInclude(i => i.TxOutput)
                 .ThenInclude(o => o.Assets)
                 .Where(t => t.Block == rollbackBlock)
                 .ToListAsync();

            if (transactions is not null)
            {
                foreach (Transaction transaction in transactions)
                {
                    // Check for rollback mint transactions and delete nft owners
                    if (transaction.Metadata is not null)
                    {
                        List<AssetClass> assetClasses = _metadataService.FindAssets(transaction, _settings.NftPolicyIds.ToList());

                        foreach (AssetClass asset in assetClasses)
                        {
                            NftOwner? owner = await _dbContext.NftOwners
                                .Where(n => n.PolicyId.ToLower() == asset.PolicyId.ToLower() &&
                                    n.TokenName.ToLower() == asset.Name.ToLower())
                                .FirstOrDefaultAsync();

                            if (owner is null) continue;

                            // If mint transaction rollback, delete entry
                            if (assetClasses.Any(a => a.PolicyId == owner.PolicyId && a.Name == owner.TokenName))
                            {
                                _dbContext.NftOwners.Remove(owner);
                            }
                        }
                    }

                    // delete other asset inputs included in the transaction
                    foreach (TxInput txInput in transaction.Inputs)
                    {
                        if (txInput.TxOutput.Assets is null) continue;
                        foreach (Asset asset in txInput.TxOutput.Assets)
                        {
                            if (_settings.NftPolicyIds.Contains(asset.PolicyId.ToLower()))
                            {

                                NftOwner? owner = await _dbContext.NftOwners
                                    .Where(n => n.PolicyId.ToLower() == asset.PolicyId.ToLower() &&
                                        n.TokenName.ToLower() == asset.Name.ToLower())
                                    .FirstOrDefaultAsync();

                                if (owner is null) continue;

                                if (owner.Address != txInput.TxOutput.Address)
                                {
                                    owner.Address = txInput.TxOutput.Address;
                                    _dbContext.NftOwners.Update(owner);
                                }
                            }
                        }
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}