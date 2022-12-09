using System.Net;
using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Common.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Conclave.Sink.Models.Oura;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.PoolRetirement)]
public class PoolRetirementReducer : OuraReducerBase
{
    private readonly ILogger<PoolRetirementReducer> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;
    public PoolRetirementReducer(
        ILogger<PoolRetirementReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        OuraPoolRetirementEvent? poolRetirementEvent = ouraEvent as OuraPoolRetirementEvent;
        if (poolRetirementEvent is not null &&
            poolRetirementEvent.Context is not null &&
            poolRetirementEvent.PoolRetirement is not null &&
            poolRetirementEvent.PoolRetirement.Pool is not null &&
            poolRetirementEvent.PoolRetirement.Epoch is not null &&
            poolRetirementEvent.Context.TxHash is not null)
        {
            Transaction? transaction = await _dbContext.Transactions
                .Where(t => t.Hash == poolRetirementEvent.Context.TxHash)
                .FirstOrDefaultAsync();
                
            if (transaction is not null &&
                transaction.Block.InvalidTransactions is not null &&
                transaction.Block.InvalidTransactions.Contains(transaction.Index))
                return;
                
            if (transaction is not null)
            {
                await _dbContext.PoolRetirements.AddAsync(new()
                {
                    Pool = _cardanoService.PoolHashToBech32(poolRetirementEvent.PoolRetirement.Pool),
                    EffectiveEpoch = (ulong)poolRetirementEvent.PoolRetirement.Epoch,
                    TxHash = poolRetirementEvent.Context.TxHash,
                    Transaction = transaction
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}