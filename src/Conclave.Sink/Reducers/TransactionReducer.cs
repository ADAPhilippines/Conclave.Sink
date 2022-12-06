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
public class TransactionReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConclaveSinkSettings _settings;

    public TransactionReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IServiceProvider serviceProvider,
        IOptions<ConclaveSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraTransactionEvent transactionEvent)
    {
        ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (transactionEvent is not null &&
            transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Transaction is not null)
        {
            Transaction? transaction = await _dbContext.Transaction
                .Where(t => t.Hash == transactionEvent.Context.TxHash)
                .FirstOrDefaultAsync();

            if (transaction is not null) return;

            Block? block = await _dbContext.Block
                .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) return;

            if (transactionEvent.Transaction.Withdrawals is not null)
            {
                foreach (Withdrawal withdrawal in transactionEvent.Transaction.Withdrawals)
                {
                    withdrawal.RewardAccount = Bech32.Encode(withdrawal.RewardAccount.HexToByteArray(),
                        AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType));
                }
            }

            await _dbContext.Transaction.AddAsync(new()
            {
                Hash = transactionEvent.Context.TxHash,
                Fee = transactionEvent.Transaction.Fee,
                Withdrawals = transactionEvent.Transaction.Withdrawals,
                Block = block
            });

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        await Task.CompletedTask;
    }
}