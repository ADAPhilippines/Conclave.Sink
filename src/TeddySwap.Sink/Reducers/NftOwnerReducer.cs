
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

[OuraReducer(OuraVariant.TxOutput)]
public class NftOwnerReducer : OuraReducerBase
{
    private readonly ILogger<NftOwnerReducer> _logger;
    private readonly IDbContextFactory<TeddySwapNftSinkDbContext> _dbContextFactory;
    private readonly TeddySwapSinkSettings _settings;

    public NftOwnerReducer(
        ILogger<NftOwnerReducer> logger,
        IDbContextFactory<TeddySwapNftSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraTxOutput txOutput)
    {

        if (txOutput is not null &&
            txOutput.Assets is not null &&
            txOutput.Address is not null)
        {
            using TeddySwapNftSinkDbContext? _dbContext = await _dbContextFactory.CreateDbContextAsync();

            foreach (OuraAsset asset in txOutput.Assets.Where(a => _settings.NftPolicyIds.Contains(a.Policy)))
            {
                if (asset.Policy is not null && asset.Asset is not null)
                {
                    NftOwner? owner = await _dbContext.NftOwners
                        .Where(n => n.Address == txOutput.Address &&
                            n.PolicyId == asset.Policy &&
                            n.TokenName == asset.Asset)
                        .FirstOrDefaultAsync();

                    if (owner is null)
                    {
                        await _dbContext.NftOwners.AddAsync(new()
                        {
                            Address = txOutput.Address,
                            PolicyId = asset.Policy,
                            TokenName = asset.Asset.ToLower(),
                        });
                    }
                    else
                    {
                        owner.Address = txOutput.Address;
                        _dbContext.NftOwners.Update(owner);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block _) => await Task.CompletedTask;
}