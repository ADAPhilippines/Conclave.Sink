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

            Block? block = await _dbContext.Blocks
                .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) throw new NullReferenceException("Block does not exist!");

            await _dbContext.Transactions.AddAsync(new()
            {
                Hash = transactionEvent.Context.TxHash,
                Fee = transactionEvent.Transaction.Fee,
                Withdrawals = transactionEvent.Transaction.Withdrawals?.Select(ouraWithdrawal => new Withdrawal
                {
                    StakeAddress = Bech32.Encode(ouraWithdrawal.RewardAccount.HexToByteArray(), AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType)),
                    Amount = ouraWithdrawal.Coin ?? 0UL
                }),
                Block = block
            });

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}