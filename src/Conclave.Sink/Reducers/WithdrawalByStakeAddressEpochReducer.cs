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

[OuraReducer(OuraVariant.Transaction)]
public class WithdrawalByStakeAddressEpochReducer : OuraReducerBase
{
    private readonly ILogger<WithdrawalByStakeAddressEpochReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly ConclaveSinkSettings _settings;
    public WithdrawalByStakeAddressEpochReducer(ILogger<WithdrawalByStakeAddressEpochReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        IOptions<ConclaveSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _cardanoService = cardanoService;
    }
    public async Task ReduceAsync(OuraTransactionEvent transactionEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Withdrawals is not null &&
            transactionEvent.Context is not null &&
            transactionEvent.Context.BlockHash is not null &&
            transactionEvent.Context.TxHash is not null)
        {
            IEnumerable<Withdrawal> withdrawals = transactionEvent.Transaction.Withdrawals;

            if (withdrawals.Any())
            {
                Block? block = await _dbContext.Block
                    .Where(b => b.BlockHash == transactionEvent.Context.BlockHash).FirstOrDefaultAsync();

                if (block is null) return;

                foreach (Withdrawal withdrawal in withdrawals)
                {
                    WithdrawalByStakeAddressEpoch? withdrawalByStakeAddressEpoch = await _dbContext.WithdrawalByStakeAddressEpoch
                        .Where(w => w.StakeAddress == withdrawal.RewardAccount &&
                            w.Transactionhash == transactionEvent.Context.BlockHash).FirstOrDefaultAsync();

                    if (withdrawalByStakeAddressEpoch is null)
                    {
                        await _dbContext.WithdrawalByStakeAddressEpoch.AddAsync(new()
                        {
                            StakeAddress = Bech32.Encode(withdrawal.RewardAccount.HexToByteArray(),
                                AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType)),
                            Transactionhash = transactionEvent.Context.TxHash,
                            Amount = withdrawal.Coin,
                            Block = block
                        });
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
        }

    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        await Task.CompletedTask;
    }
}