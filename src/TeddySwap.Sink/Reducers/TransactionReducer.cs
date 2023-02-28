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
using PeterO.Cbor2;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.Transaction)]
public class TransactionReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TransactionReducer> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IServiceProvider _serviceProvider;
    private readonly TeddySwapSinkSettings _settings;
    private readonly IEnumerable<IOuraReducer> _reducers;

    public TransactionReducer(
        ILogger<TransactionReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IServiceProvider serviceProvider,
        IEnumerable<IOuraReducer> reducers,
        IOptions<TeddySwapSinkSettings> settings)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _reducers = reducers;
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

            // Add Inputs Here Instead of Events to Avoid Null Reference
            if (transactionEvent.Transaction.Inputs is not null)
            {
                foreach (OuraTxInput input in transactionEvent.Transaction.Inputs)
                {
                    TxOutput? txOutput = await _dbContext.TxOutputs
                        .Where(txOutput => txOutput.TxHash == input.TxHash && txOutput.Index == input.Index).FirstOrDefaultAsync();

                    if (txOutput is not null)
                    {
                        await _dbContext.TxInputs.AddAsync(new()
                        {
                            TxHash = input.TxHash,
                            Transaction = transaction,
                            TxOutputHash = input.TxHash,
                            TxOutputIndex = input.Index,
                            InlineDatum = txOutput.InlineDatum,
                            TxOutput = txOutput
                        });
                    }
                    else
                    {
                        await _dbContext.TxInputs.AddAsync(new()
                        {
                            TxHash = input.TxHash,
                            Transaction = transaction,
                            // GENESIS TX HACK
                            TxOutput = new TxOutput
                            {
                                Transaction = new Transaction
                                {
                                    Hash = $"GENESIS_{transaction.Hash}_{transactionEvent.Fingerprint + input.Index}",
                                    Block = transaction.Block
                                },
                                Address = "GENESIS"
                            }
                        });
                    }
                }

                // Add Outputs Here Instead of Events to Avoid Null Reference
                if (transactionEvent.Transaction.Outputs is not null)
                {
                    for (int i = 0; i < transactionEvent.Transaction.Outputs.Count(); i++)
                    {
                        OuraTxOutput output = transactionEvent.Transaction.Outputs.ToList()[i];

                        if (output is not null &&
                            output.Amount is not null &&
                            output.Address is not null)
                        {
                            TxOutput newTxOutput = new()
                            {
                                Amount = (ulong)output.Amount,
                                Address = output.Address,
                                Index = (ulong)i,
                                InlineDatum = CBORObject.(output.InlineDatum)
                            };

                            newTxOutput = newTxOutput with { Transaction = transaction, TxHash = transaction.Hash };

                            EntityEntry<TxOutput> insertResult = await _dbContext.TxOutputs.AddAsync(newTxOutput);
                            await _dbContext.SaveChangesAsync();

                            if (output.Assets is not null && output.Assets.Count() > 0)
                            {
                                await _dbContext.AddRangeAsync(output.Assets.Select(ouraAsset =>
                                {
                                    return new Asset
                                    {
                                        PolicyId = ouraAsset.Policy ?? string.Empty,
                                        Name = ouraAsset.Asset ?? string.Empty,
                                        Amount = ouraAsset.Amount ?? 0,
                                        TxOutput = insertResult.Entity
                                    };
                                }));
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();

                // Call Order Reducer
                if (block.InvalidTransactions is not null &&
                    !block.InvalidTransactions.ToList().Contains(transaction.Index))
                {
                    OrderReducer? reducer = _reducers.Where(r => r is BlockReducer).FirstOrDefault() as OrderReducer;

                    if (reducer is not null)
                    {
                        await reducer.HandleReduceAsync(transactionEvent);
                    }
                }
            }
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}