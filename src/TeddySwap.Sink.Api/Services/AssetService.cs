using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.CardanoDbSync;
using TeddySwap.Common.Enums;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Models;
using TeddySwap.Sink.Data;

namespace TeddySwap.Sink.Api.Services;

public class AssetService
{
    private readonly ILogger<AssetService> _logger;
    private readonly TeddySwapNftSinkDbContext _dbContext;
    private readonly TeddySwapITNRewardSettings _settings;

    public AssetService(
        ILogger<AssetService> logger,
        TeddySwapNftSinkDbContext dbContext,
        IOptions<TeddySwapITNRewardSettings> settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<PaginatedAssetResponse> GetNftOwnerAsync(int offset, int limit, string address, string policyId)
    {
        var nftOwnerQuery = _dbContext.NftOwners
            .Where(no => no.Address == address && no.PolicyId == policyId.ToLower());

        var totalNfts = await nftOwnerQuery.CountAsync();

        List<AssetResponse> paginatedNfts = await nftOwnerQuery
            .Skip(offset)
            .Take(limit)
            .Select(no => new AssetResponse()
            {
                Name = no.TokenName,
                AsciiName = Encoding.ASCII.GetString(Convert.FromHexString(no.TokenName)),
                Amount = 1
            })
            .ToListAsync();

        return new()
        {
            TotalCount = totalNfts,
            PolicyId = policyId,
            Result = paginatedNfts,
            Address = address
        };
    }

    public async Task<AssetMetadataResponse?> GetNftMetadataAsync(string policyId, string tokenName)
    {
        return await _dbContext.MintTransactions
            .Where(mtx => mtx.PolicyId == policyId.ToLower() && mtx.TokenName == tokenName.ToLower())
            .Select(m => new AssetMetadataResponse()
            {
                PolicyId = m.PolicyId,
                TokenName = m.TokenName,
                AsciiTokenName = m.AsciiTokenName,
                Metadata = m.Metadata
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<AssetMetadataResponse>?> GetNftMetadataAsync(List<string> assets)
    {
        return await _dbContext.MintTransactions
            .Where(mtx => assets.Contains(mtx.PolicyId + mtx.TokenName))
            .Select(m => new AssetMetadataResponse()
            {
                PolicyId = m.PolicyId,
                TokenName = m.TokenName,
                AsciiTokenName = m.AsciiTokenName,
                Metadata = m.Metadata
            })
            .ToListAsync();
    }
}