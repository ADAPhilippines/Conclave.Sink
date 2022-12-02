using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRegistration)]
public class RewardAddressByPoolPerEpochReducer : OuraReducerBase
{
    private readonly ILogger<RewardAddressByPoolPerEpochReducer> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly ConclaveSinkSettings _settings;
    private readonly CardanoService _cardanoService;

    public RewardAddressByPoolPerEpochReducer(ILogger<RewardAddressByPoolPerEpochReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        IOptions<ConclaveSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _cardanoService = cardanoService;
    }
    public async Task ReduceAsync(OuraPoolRegistrationEvent poolRegistrationEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (poolRegistrationEvent is not null &&
            poolRegistrationEvent.Context is not null &&
            poolRegistrationEvent.PoolRegistration is not null &&
            poolRegistrationEvent.PoolRegistration.RewardAccount is not null &&
            poolRegistrationEvent.Context.TxHash is not null &&
            poolRegistrationEvent.Context.TxIdx is not null
            )
        {
            string poolId = _cardanoService.PoolHashToBech32(poolRegistrationEvent.PoolRegistration.Operator);
            RewardAddressByPoolPerEpoch? entry = await _dbContext.RewardAddressByPoolPerEpoch
                .Where((rapp) => rapp.RewardAddress == poolRegistrationEvent.PoolRegistration.RewardAccount &&
                    rapp.TxHash == poolRegistrationEvent.Context.TxHash &&
                    rapp.TxIndex == (ulong)poolRegistrationEvent.Context.TxIdx &&
                    rapp.PoolId == poolId)
                .FirstOrDefaultAsync();

            if (entry is not null) return;

            Block? block = await _dbContext.Block.Where(b => b.BlockHash == poolRegistrationEvent.Context.BlockHash).FirstOrDefaultAsync();

            string prefix = AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType);
            string rewardAddress = Bech32.Encode(poolRegistrationEvent.PoolRegistration.RewardAccount.HexToByteArray(), prefix);

            await _dbContext.RewardAddressByPoolPerEpoch.AddAsync(new RewardAddressByPoolPerEpoch
            {
                PoolId = poolId,
                RewardAddress = rewardAddress,
                TxHash = poolRegistrationEvent.Context.TxHash,
                TxIndex = (ulong)poolRegistrationEvent.Context.TxIdx,
                Block = block
            });

            await _dbContext.SaveChangesAsync();
        }
    }


    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        IEnumerable<RewardAddressByPoolPerEpoch>? rewardAddressByPoolPerEpochs = _dbContext.RewardAddressByPoolPerEpoch.Include(ra => ra.Block)
            .Where(ra => ra.Block!.BlockHash == rollbackBlock.BlockHash);
        if (rewardAddressByPoolPerEpochs is null) return;
        _dbContext.RewardAddressByPoolPerEpoch.RemoveRange(rewardAddressByPoolPerEpochs);
        await _dbContext.SaveChangesAsync();
    }
}
