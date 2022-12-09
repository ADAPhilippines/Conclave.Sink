using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Conclave.Common.Models.Entities;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Models.Oura;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class WithdrawalByStakeEpochReducer : OuraReducerBase
{
    private readonly ILogger<WithdrawalByStakeEpochReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly ConclaveSinkSettings _settings;

    public WithdrawalByStakeEpochReducer(ILogger<WithdrawalByStakeEpochReducer> logger,
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
        if (transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Withdrawals is not null &&
            transactionEvent.Context is not null &&
            transactionEvent.Context.BlockHash is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Context.Slot is not null &&
            transactionEvent.Context.TxIdx is not null)
        {
            using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            IEnumerable<OuraWithdrawal> withdrawals = transactionEvent.Transaction.Withdrawals;

            if (withdrawals.Any())
            {
                Block? block = await _dbContext.Blocks
                    .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                    .FirstOrDefaultAsync();

                if (block is null) throw new NullReferenceException("Block does not exist!");

                if (block.InvalidTransactions is not null &&
                    block.InvalidTransactions.Contains((ulong)transactionEvent.Context.TxIdx)) return;

                ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)transactionEvent.Context.Slot);
                foreach (OuraWithdrawal withdrawal in withdrawals)
                {
                    if (withdrawal.Coin is null || withdrawal.Coin <= 0) continue;

                    WithdrawalByStakeEpoch? withdrawalByStakeEpoch = await _dbContext.WithdrawalByStakeEpoch
                        .Where(w => w.StakeAddress == withdrawal.RewardAccount &&
                            w.Epoch == epoch).FirstOrDefaultAsync();

                    if (withdrawalByStakeEpoch is not null)
                    {
                        withdrawalByStakeEpoch.Amount += (ulong)withdrawal.Coin;
                    }
                    else
                    {
                        string stakeAddress = Bech32.Encode(withdrawal.RewardAccount.HexToByteArray(),
                            AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType));
                        ulong previousWithdrawal = await _dbContext.WithdrawalByStakeEpoch
                            .Where(w => w.StakeAddress == stakeAddress &&
                                w.Epoch < epoch)
                            .OrderByDescending(w => w.Epoch)
                            .Select(w => w.Amount)
                            .FirstOrDefaultAsync();

                        await _dbContext.WithdrawalByStakeEpoch.AddAsync(new()
                        {
                            StakeAddress = stakeAddress,
                            Amount = (ulong)withdrawal.Coin + previousWithdrawal,
                            Epoch = epoch
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
        }

    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        IEnumerable<Transaction>? transactions = await _dbContext.Transactions
            .Include(t => t.Withdrawals)
            .Include(t => t.Block)
            .Where(t => t.Block == rollbackBlock && t.Withdrawals.Any())
            .ToListAsync();

        if (transactions is null) return;

        foreach (Transaction transaction in transactions)
        {
            if ((transaction.Block.InvalidTransactions is not null && transaction.Block.InvalidTransactions.Contains(transaction.Index)) ||
                transaction.Withdrawals is null) continue;

            foreach (Withdrawal withdrawal in transaction.Withdrawals)
            {
                if (withdrawal.Amount <= 0) continue;

                WithdrawalByStakeEpoch? withdrawalByStakeEpoch = await _dbContext.WithdrawalByStakeEpoch
                    .Where(w => w.StakeAddress == withdrawal.StakeAddress &&
                        w.Epoch == rollbackBlock.Epoch)
                    .FirstOrDefaultAsync();

                if (withdrawalByStakeEpoch is null) throw new NullReferenceException("WithdrawalByStakeEpoch does not exist!");

                ulong previousWithdrawal = await _dbContext.WithdrawalByStakeEpoch
                    .Where(w => w.StakeAddress == withdrawal.StakeAddress && w.Epoch < rollbackBlock.Epoch)
                    .OrderByDescending(w => w.Epoch)
                    .Select(w => w.Amount)
                    .FirstOrDefaultAsync();

                withdrawalByStakeEpoch.Amount -= withdrawal.Amount;

                if (withdrawalByStakeEpoch.Amount <= 0 || withdrawalByStakeEpoch.Amount <= previousWithdrawal)
                    _dbContext.WithdrawalByStakeEpoch.Remove(withdrawalByStakeEpoch);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}