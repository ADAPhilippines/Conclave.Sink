using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Utilities;
using TeddySwap.Sink.Data;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Conclave.Sink.Reducers;

public class OrderReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IServiceProvider _serviceProvider;
    private readonly TeddySwapSinkSettings _settings;

    public OrderReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IServiceProvider serviceProvider,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    public async Task ReduceAsync(OuraTransactionEvent transactionEvent)
    {
        if (transactionEvent is not null &&
            transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Fee is not null &&
            transactionEvent.Context.TxIdx is not null)
        {
            TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Block? block = await _dbContext.Blocks
                .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) throw new NullReferenceException("Block does not exist!");

            Transaction? transaction = await _dbContext.Transactions
                .Include(t => t.Inputs)
                .ThenInclude(i => i.TxOutput)
                .Include(t => t.Outputs)
                .Where(t => t.Hash == transactionEvent.Context.TxHash)
                .FirstOrDefaultAsync();

            if (transaction is null) throw new NullReferenceException("Transaction does not exist!");

            List<TxOutput>? inputs = transaction.Inputs.Select(i => i.TxOutput).ToList();
            List<TxOutput>? outputs = transaction.Outputs.ToList();

            List<string> validators = new()
            {
                _settings.DepositAddress,
                _settings.SwapAddress,
                _settings.RedeemAddress
            };

            // Find Validator Utxos
            TxOutput? poolInput = inputs.Where(i => i.Address == _settings.PoolAddress).FirstOrDefault();
            TxOutput? orderInput = inputs.Where(i => validators.Contains(i.Address)).FirstOrDefault();

            // Return if not a TeddySwap transaction
            if (poolInput is null || orderInput is null) return;
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}