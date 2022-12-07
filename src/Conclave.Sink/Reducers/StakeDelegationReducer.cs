using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.StakeDelegation)]
public class StakeDelegationReducer : OuraReducerBase
{
    private readonly ILogger<StakeDelegationReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly ConclaveSinkSettings _settings;
    public StakeDelegationReducer(ILogger<StakeDelegationReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        IOptions<ConclaveSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _cardanoService = cardanoService;
    }
    public async Task ReduceAsync(OuraStakeDelegationEvent stakeDelegationEvent)
    {

        if (stakeDelegationEvent is not null &&
            stakeDelegationEvent.Context is not null &&
            stakeDelegationEvent.StakeDelegation is not null &&
            stakeDelegationEvent.StakeDelegation.Credential is not null &&
            stakeDelegationEvent.StakeDelegation.PoolHash is not null &&
            stakeDelegationEvent.Context.TxIdx is not null &&
            stakeDelegationEvent.Context.TxHash is not null)
        {
            using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Transaction? transaction = await _dbContext.Transactions
                .Where(t => t.Hash == stakeDelegationEvent.Context.TxHash)
                .FirstOrDefaultAsync();

            if (transaction is null) throw new NullReferenceException("Transaction does not exist!");

            string stakeKeyHash = string.IsNullOrEmpty(stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash) ?
                stakeDelegationEvent.StakeDelegation.Credential.Scripthash :
                stakeDelegationEvent.StakeDelegation.Credential.AddrKeyHash;

            string stakeAddress = AddressUtility.GetRewardAddress(stakeKeyHash.HexToByteArray(), _settings.NetworkType).ToString();
            string poolId = _cardanoService.PoolHashToBech32(stakeDelegationEvent.StakeDelegation.PoolHash);

            await _dbContext.StakeDelegations.AddAsync(new()
            {
                StakeAddress = stakeAddress,
                PoolId = poolId,
                TxHash = stakeDelegationEvent.Context.TxHash,
                Transaction = transaction
            });

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}