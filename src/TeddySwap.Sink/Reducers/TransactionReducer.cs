using TeddySwap.Sink.Data;
using TeddySwap.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class TransactionReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;


    public TransactionReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
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

            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}