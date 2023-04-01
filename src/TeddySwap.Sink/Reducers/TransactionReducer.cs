using System.Text.Json;
using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
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
public class TransactionReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkCoreDbContext> _dbContextFactory;
    private readonly ByteArrayService _byteArrayService;
    private readonly TeddySwapSinkSettings _settings;

    public TransactionReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<TeddySwapSinkCoreDbContext> dbContextFactory,
        ByteArrayService byteArrayService,
        IOptions<TeddySwapSinkSettings> settings
        )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _byteArrayService = byteArrayService;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraTransaction transaction)
    {
        if (transaction is not null &&
            transaction.Context is not null &&
            transaction.Fee is not null &&
            transaction.Hash is not null)
        {
            using TeddySwapSinkCoreDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Block? block = await _dbContext.Blocks
                .Where(b => b.BlockHash == transaction.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) throw new NullReferenceException("Block does not exist!");

            Transaction? existingTransaction = await _dbContext.Transactions
                .Where(t => t.Hash == transaction.Hash)
                .FirstOrDefaultAsync();

            if (existingTransaction is not null) return;

            Transaction newTransaction = new()
            {
                Hash = transaction.Hash,
                Fee = (ulong)transaction.Fee,
                Index = (ulong)transaction.Index,
                Block = block,
                Blockhash = block.BlockHash,
                Metadata = JsonSerializer.Serialize(transaction.Metadata),
                HasCollateralOutput = transaction.HasCollateralOutput
            };

            if (transaction.Withdrawals is not null)
            {
                foreach (OuraWithdrawal ouraWithdrawal in transaction.Withdrawals)
                {
                    if (ouraWithdrawal.RewardAccount is null) continue;
                    await _dbContext.Withdrawals.AddAsync(new()
                    {
                        Amount = ouraWithdrawal.Coin ?? 0,
                        StakeAddress = Bech32.Encode(_byteArrayService.HexToByteArray(ouraWithdrawal.RewardAccount), AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType)),
                        Transaction = newTransaction,
                    });
                }
            }

            await _dbContext.Transactions.AddAsync(newTransaction);
            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}