using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput, OuraVariant.TxOutput)]
public class BalanceByStakeEpochReducer : OuraReducerBase
{
    private readonly ILogger<BalanceByStakeEpoch> _logger;
    private readonly IDbContextFactory<TeddySwapFisoSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;

    public BalanceByStakeEpochReducer(
        ILogger<BalanceByStakeEpoch> logger,
        IDbContextFactory<TeddySwapFisoSinkDbContext> dbContextFactory,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        await (ouraEvent.Variant switch
        {
            OuraVariant.TxInput => Task.Run(async () =>
            {
                OuraTxInputEvent? txInputEvent = ouraEvent as OuraTxInputEvent;
                if (txInputEvent is not null &&
                    txInputEvent.TxInput is not null &&
                    txInputEvent.Context is not null &&
                    txInputEvent.Context.Slot is not null)
                {
                    TxOutput? input = await _dbContext.TxOutputs
                        .Include(txOut => txOut.Transaction)
                        .ThenInclude(transaction => transaction.Block)
                        .Where(txOut => txOut.TxHash == txInputEvent.TxInput.TxHash && txOut.Index == txInputEvent.TxInput.Index)
                        .FirstOrDefaultAsync();

                    if (input is not null)
                    {
                        string? stakeAddress = _cardanoService.TryGetStakeAddress(input.Address);
                        ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txInputEvent.Context.Slot);

                        if (stakeAddress is null) return;

                        BalanceByStakeEpoch? entry = await _dbContext.BalanceByStakeEpoch
                            .Where((bbae) => bbae.StakeAddress == stakeAddress && bbae.Epoch == epoch)
                            .FirstOrDefaultAsync();

                        if (entry is not null)
                        {
                            entry.Balance -= input.Amount;

                            if (entry.Balance <= 0UL)
                            {
                                _dbContext.BalanceByStakeEpoch.Remove(entry);
                            }
                        }
                        else
                        {
                            await _dbContext.BalanceByStakeEpoch.AddAsync(new()
                            {
                                StakeAddress = stakeAddress,
                                Balance = await GetLastEpochBalanceByStakeAsync(stakeAddress, epoch) - input.Amount,
                                Epoch = epoch
                            });
                        }

                        await _dbContext.SaveChangesAsync();
                    }
                }
            }),
            OuraVariant.TxOutput => Task.Run(async () =>
            {
                OuraTxOutputEvent? txOutputEvent = ouraEvent as OuraTxOutputEvent;
                if (txOutputEvent is not null &&
                    txOutputEvent.TxOutput is not null &&
                    txOutputEvent.TxOutput.Amount is not null &&
                    txOutputEvent.TxOutput.Address is not null &&
                    txOutputEvent.Context is not null &&
                    txOutputEvent.Context.Slot is not null)
                {
                    string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutputEvent.TxOutput.Address);
                    ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txOutputEvent.Context.Slot);
                    ulong amount = (ulong)txOutputEvent.TxOutput.Amount;

                    if (stakeAddress is null) return;

                    BalanceByStakeEpoch? entry = await _dbContext.BalanceByStakeEpoch
                        .Where((bbae) => bbae.StakeAddress == stakeAddress && bbae.Epoch == epoch)
                        .FirstOrDefaultAsync();

                    if (entry is not null)
                    {
                        entry.Balance += amount;
                    }
                    else
                    {
                        await _dbContext.BalanceByStakeEpoch.AddAsync(new()
                        {
                            StakeAddress = stakeAddress,
                            Balance = await GetLastEpochBalanceByStakeAsync(stakeAddress, epoch) + amount,
                            Epoch = epoch
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }),
            _ => Task.Run(() => { })
        });
    }

    public async Task<ulong> GetLastEpochBalanceByStakeAsync(string stakeAddress, ulong? epoch)
    {
        using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await _dbContext.BalanceByStakeEpoch
            .Where((bbae) => (bbae.StakeAddress == stakeAddress && bbae.Epoch < epoch))
            .OrderByDescending(s => s.Epoch)
            .Select(s => s.Balance)
            .FirstOrDefaultAsync();
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        IEnumerable<TxInput> consumed = await _dbContext.TxInputs
            .Include(txInput => txInput.Transaction)
            .ThenInclude(transaction => transaction.Block)
            .Include(txInput => txInput.TxOutput)
            .Where(txInput => txInput.Transaction.Block == rollbackBlock)
            .ToListAsync();

        IEnumerable<TxOutput> produced = await _dbContext.TxOutputs
            .Include(txOutput => txOutput.Transaction)
            .ThenInclude(transaction => transaction.Block)
            .Where(txOutput => txOutput.Transaction.Block == rollbackBlock)
            .ToListAsync();

        IEnumerable<Task> consumeTasks = consumed.ToList().Select(txInput => Task.Run(async () =>
        {
            string? stakeAddress = _cardanoService.TryGetStakeAddress(txInput.TxOutput.Address);

            if (stakeAddress is not null)
            {
                BalanceByStakeEpoch? entry = await _dbContext.BalanceByStakeEpoch
                    .Where((bbsae) => (bbsae.StakeAddress == stakeAddress) && (bbsae.Epoch == rollbackBlock.Epoch))
                    .FirstOrDefaultAsync();

                if (entry is not null)
                {
                    entry.Balance += txInput.TxOutput.Amount;
                }
            }
        }));

        foreach (Task consumeTask in consumeTasks) await consumeTask;

        IEnumerable<Task> produceTasks = produced.ToList().Select(txOutput => Task.Run(async () =>
        {
            string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutput.Address);

            if (stakeAddress is not null)
            {
                BalanceByStakeEpoch? entry = await _dbContext.BalanceByStakeEpoch
                    .Where((bba) => (bba.StakeAddress == stakeAddress) && (bba.Epoch == rollbackBlock.Epoch))
                    .FirstOrDefaultAsync();

                if (entry is not null)
                {
                    ulong prevEpochBalance = await GetLastEpochBalanceByStakeAsync(stakeAddress, rollbackBlock.Epoch);

                    entry.Balance -= txOutput.Amount;

                    if (entry.Balance <= 0 || entry.Balance <= prevEpochBalance)
                    {
                        _dbContext.Remove(entry);
                    }
                }
            };
        }));

        foreach (Task produceTask in produceTasks) await produceTask;

        await _dbContext.SaveChangesAsync();
    }
}