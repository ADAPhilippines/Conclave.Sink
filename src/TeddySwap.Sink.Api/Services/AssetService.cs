using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.CardanoDbSync;
using TeddySwap.Common.Models.Enums;
using TeddySwap.Common.Models.Request;
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

    public byte[] HexToByteArray(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new Exception("The binary key cannot have an odd number of digits");

        byte[] arr = new byte[hex.Length >> 1];

        for (int i = 0; i < hex.Length >> 1; ++i)
        {
            arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
        }

        return arr;
    }

    public int GetHexVal(char hex)
    {
        int val = (int)hex;
        //For uppercase A-F letters:
        //return val - (val < 58 ? 48 : 55);
        //For lowercase a-f letters:
        //return val - (val < 58 ? 48 : 87);
        //Or the two combined, but a bit slower:
        return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }


    public async Task<PaginatedAssetResponse> GetAssetsAsync(PaginatedAssetRequest request)
    {
        var unspentTxOuts = await _dbContext.TxOuts
            .Where(o => o.Address == request.Address && !_dbContext.TxIns.Any(i => i.TxOutId == o.TxId && i.TxOutIndex == o.Index))
            .OrderBy(o => o.Id)
            .ToListAsync();

        var policyBytes = HexToByteArray(request.PolicyId);

        var maTxOuts = await _dbContext.MaTxOuts
            .Include(maTxOut => maTxOut.TxOut)
            .Include(maTxOut => maTxOut.IdentNavigation)
            .Where(maTxOut => maTxOut.IdentNavigation.Policy.SequenceEqual(policyBytes))
            .Where(maTxOut => unspentTxOuts.Contains(maTxOut.TxOut))
            .ToListAsync();

        var assets = maTxOuts
            .GroupBy(maTxOut => new
            {
                Name = Encoding.UTF8.GetString(maTxOut.IdentNavigation.Name),
                PolicyId = BitConverter.ToString(maTxOut.IdentNavigation.Policy).Replace("-", string.Empty).ToLower()
            })
            .Select(g => new AssetResponse
            {
                Name = g.Key.Name,
                Amount = (ulong)g.Sum(maTxOut => maTxOut.Quantity)
            })
            .ToList();

        return new PaginatedAssetResponse()
        {
            PolicyId = request.PolicyId,
            Address = request.Address,
            TotalCount = assets.Count,
            Result = assets.Skip(request.Offset).Take(request.Limit).ToList()
        };
    }

    public async Task<AssetMetadataResponse> GetAssetsMetadataAsync(AssetMetadataRequest request)
    {


        return new AssetMetadataResponse();
    }

}