using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class WithdrawalByStakeEpochReducer : OuraReducerBase
{
    private readonly ILogger<WithdrawalByStakeEpochReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkCoreDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly TeddySwapSinkSettings _settings;

    public WithdrawalByStakeEpochReducer(ILogger<WithdrawalByStakeEpochReducer> logger,
        IDbContextFactory<TeddySwapSinkCoreDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraTransaction transaction)
    {
        if (transaction is not null &&
            transaction.Withdrawals is not null &&
            transaction.Context is not null &&
            transaction.Context.BlockHash is not null &&
            transaction.Hash is not null &&
            transaction.Context.Slot is not null)
        {
            using TeddySwapSinkCoreDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            IEnumerable<OuraWithdrawal> withdrawals = transaction.Withdrawals;

            if (withdrawals.Any())
            {
                Block? block = await _dbContext.Blocks
                    .Where(b => b.BlockHash == transaction.Context.BlockHash)
                    .FirstOrDefaultAsync();

                if (block is null) throw new NullReferenceException("Block does not exist!");

                if (block.InvalidTransactions is not null &&
                    block.InvalidTransactions.Contains((ulong)transaction.Index)) return;

                ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)transaction.Context.Slot);
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
        using TeddySwapSinkCoreDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

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