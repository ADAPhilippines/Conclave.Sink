using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PeterO.Cbor2;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.TxOutput)]
public class TxOutputReducer : OuraReducerBase, IOuraCoreReducer
{
    private readonly ILogger<TxOutputReducer> _logger;
    private IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    public TxOutputReducer(
        ILogger<TxOutputReducer> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReduceAsync(OuraTxOutputEvent txOutputEvent)
    {
        using TeddySwapSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (txOutputEvent is not null &&
        txOutputEvent.Context is not null &&
         txOutputEvent.TxOutput is not null &&
         txOutputEvent.Context.TxHash is not null &&
         txOutputEvent.Context.OutputIdx is not null &&
         txOutputEvent.Context.BlockHash is not null &&
         txOutputEvent.TxOutput.Amount is not null &&
         txOutputEvent.TxOutput.Address is not null)
        {
            Transaction? tx = await _dbContext.Transactions.Include(tx => tx.Block).Where(tx => tx.Hash == txOutputEvent.Context.TxHash).FirstOrDefaultAsync();
            if (tx is not null)
            {
                TxOutput newTxOutput = new()
                {
                    Amount = (ulong)txOutputEvent.TxOutput.Amount,
                    Address = txOutputEvent.TxOutput.Address,
                    Index = (ulong)txOutputEvent.Context.OutputIdx,
                    InlineDatum = CBORObject.FromJSONBytes(JsonSerializer.SerializeToUtf8Bytes(txOutputEvent.TxOutput.InlineDatum)).EncodeToBytes()
                };

                newTxOutput = newTxOutput with { Transaction = tx, TxHash = tx.Hash };

                EntityEntry<TxOutput> insertResult = await _dbContext.TxOutputs.AddAsync(newTxOutput);
                await _dbContext.SaveChangesAsync();

                if (txOutputEvent.TxOutput.Assets is not null && txOutputEvent.TxOutput.Assets.Count() > 0)
                {
                    await _dbContext.AddRangeAsync(txOutputEvent.TxOutput.Assets.Select(ouraAsset =>
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

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}