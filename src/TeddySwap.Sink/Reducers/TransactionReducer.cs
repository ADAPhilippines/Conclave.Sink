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
        if (transactionEvent is not null &&
            transactionEvent.Context is not null &&
            transactionEvent.Context.TxHash is not null &&
            transactionEvent.Transaction is not null &&
            transactionEvent.Transaction.Fee is not null &&
            transactionEvent.Context.TxIdx is not null)
        {
            ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

            Block? block = await _dbContext.Blocks
                .Where(b => b.BlockHash == transactionEvent.Context.BlockHash)
                .FirstOrDefaultAsync();

            if (block is null) throw new NullReferenceException("Block does not exist!");

            Transaction transaction = new()
            {
                Hash = transactionEvent.Context.TxHash,
                Fee = (ulong)transactionEvent.Transaction.Fee,
                Index = (ulong)transactionEvent.Context.TxIdx,
                Block = block
            };


            EntityEntry<Transaction> transactionEntry = await _dbContext.Transactions.AddAsync(transaction);

            // Record collateral input if available
            if (transactionEvent.Transaction.CollateralInputs is not null)
            {
                List<CollateralTxInput> collateralInputs = new List<CollateralTxInput>();
                foreach (OuraTxInput ouraTxInput in transactionEvent.Transaction.CollateralInputs)
                {
                    TxOutput? txInputOutput = await _dbContext.TxOutputs
                        .Where(txOutput => txOutput.TxHash == ouraTxInput.TxHash && txOutput.Index == ouraTxInput.Index)
                        .FirstOrDefaultAsync();

                    if (txInputOutput is not null)
                    {
                        collateralInputs.Add(new()
                        {
                            TxOutput = txInputOutput,
                            Transaction = transactionEntry.Entity
                        });
                    }
                    else
                    {
                        collateralInputs.Add(new()
                        {
                            Transaction = transactionEntry.Entity,
                            // GENESIS TX HACK
                            TxOutput = new TxOutput
                            {
                                Transaction = new Transaction
                                {
                                    Hash = $"GENESIS_{transaction.Hash}_{transactionEvent.Fingerprint}",
                                    Block = transaction.Block
                                },
                                Address = "GENESIS"
                            }
                        });
                    }
                }
                await _dbContext.CollateralTxInputs.AddRangeAsync(collateralInputs);
            }


            // If Transaction is invalid record, collateral output
            if (block.InvalidTransactions is not null &&
                transactionEvent.Transaction.CollateralOutput is not null &&
                transactionEvent.Transaction.CollateralOutput.Address is not null &&
                transactionEvent.Transaction.CollateralOutput.Amount is not null &&
                transactionEvent.Transaction.CollateralOutput.Assets is not null &&
                block.InvalidTransactions.ToList().Contains(transaction.Index))
            {
                CollateralTxOutput collateralOutput = new()
                {
                    Transaction = transaction,
                    Index = 0,
                    Address = transactionEvent.Transaction.CollateralOutput.Address,
                    Amount = (ulong)transactionEvent.Transaction.CollateralOutput.Amount,
                    Assets = transactionEvent.Transaction.CollateralOutput.Assets
                                 .Select(asset => new Asset { PolicyId = asset.Policy, Name = asset.Asset, Amount = asset.Amount ?? 0 })
                };

                await _dbContext.CollateralTxOutputs.AddAsync(collateralOutput);
            }

            if (transactionEvent.Transaction.Withdrawals is not null)
            {
                foreach (OuraWithdrawal ouraWithdrawal in transactionEvent.Transaction.Withdrawals)
                {
                    await _dbContext.Withdrawals.AddAsync(new()
                    {
                        Amount = ouraWithdrawal.Coin ?? 0,
                        StakeAddress = Bech32.Encode(ouraWithdrawal.RewardAccount.HexToByteArray(), AddressUtility.GetPrefix(AddressType.Reward, _settings.NetworkType)),
                        Transaction = transactionEntry.Entity,
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}