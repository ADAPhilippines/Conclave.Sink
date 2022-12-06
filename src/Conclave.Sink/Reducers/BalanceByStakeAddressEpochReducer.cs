using Conclave.Sink.Data;
using Conclave.Sink.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Microsoft.EntityFrameworkCore;
using CardanoSharp.Wallet.Extensions.Models;
using Conclave.Sink.Services;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput, OuraVariant.TxOutput)]
public class BalanceByStakeAddressEpochReducer : OuraReducerBase
{
    private readonly ILogger<BalanceByStakeAddressEpoch> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly CardanoService _cardanoService;

    public BalanceByStakeAddressEpochReducer(
        ILogger<BalanceByStakeAddressEpoch> logger,
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
                    TxOutput? input = await _dbContext.TxOutput.Include(i => i.Block)
                        .Where(txOut => (txOut.TxHash == txInputEvent.TxInput.TxHash) && (txOut.Index == txInputEvent.TxInput.Index))
                        .FirstOrDefaultAsync();

                    if (input is not null)
                    {
                        Address? stakeAddress = TryGetStakeAddress(new Address(input.Address));
                        ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txInputEvent.Context.Slot);

                        if (stakeAddress is null) return;

                        BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                            .Where((bbae) => (bbae.StakeAddress == stakeAddress.ToString()) && (bbae.Epoch == epoch))
                            .FirstOrDefaultAsync();

                        if (entry is not null)
                        {
                            entry.Balance -= input.Amount;
                        }
                        else
                        {
                            BalanceByStakeAddressEpoch? lastEpochBalance = await GetLastEpochBalanceByStakeAddressAsync(stakeAddress.ToString(), epoch);
                            ulong balance = lastEpochBalance is null ? input.Amount : (lastEpochBalance.Balance - input.Amount);

                            await _dbContext.BalanceByStakeAddressEpoch.AddAsync(new()
                            {
                                StakeAddress = stakeAddress.ToString(),
                                Balance = balance,
                                Epoch = epoch
                            });
                        }
                    }

                    await _dbContext.SaveChangesAsync();
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
                    Address? stakeAddress = TryGetStakeAddress(new Address(txOutputEvent.TxOutput.Address));
                    ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txOutputEvent.Context.Slot);
                    ulong amount = (ulong)txOutputEvent.TxOutput.Amount;

                    if (stakeAddress is null) return;

                    BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                        .Where((bbae) => (bbae.StakeAddress == stakeAddress.ToString()) && (bbae.Epoch == epoch))
                        .FirstOrDefaultAsync();

                    if (entry is not null)
                    {
                        entry.Balance += amount;
                    }
                    else
                    {
                        BalanceByStakeAddressEpoch? previousEntry = await GetLastEpochBalanceByStakeAddressAsync(stakeAddress.ToString(), epoch);

                        ulong balance = previousEntry is null ? (ulong)txOutputEvent.TxOutput.Amount : (previousEntry.Balance + amount);

                        await _dbContext.BalanceByStakeAddressEpoch.AddAsync(new()
                        {
                            StakeAddress = stakeAddress.ToString(),
                            Balance = balance,
                            Epoch = epoch
                        });
                    }
                    
                    await _dbContext.SaveChangesAsync();
                }
            }),
            _ => Task.Run(() => { })
        });
    }

    private Address? TryGetStakeAddress(Address paymentAddress)
    {
        try
        {
            return paymentAddress.GetStakeAddress();
        }
        catch { }
        return null;
    }

    public async Task<BalanceByStakeAddressEpoch?> GetLastEpochBalanceByStakeAddressAsync(string stakeAddress, ulong? epoch)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<BalanceByStakeAddressEpoch> sortedEpochBalances = await _dbContext.BalanceByStakeAddressEpoch
            .Where((bbae) => (bbae.StakeAddress == stakeAddress && bbae.Epoch < epoch))
            .OrderByDescending(s => s.Epoch).ToListAsync();

        BalanceByStakeAddressEpoch? lastEpoch = sortedEpochBalances.FirstOrDefault();

        return lastEpoch;
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();

        IEnumerable<TxInput> consumed = await _dbContext.TxInput
            .Include(txInput => txInput.TxOutput)
            .Where(txInput => txInput.Block == rollbackBlock)
            .ToListAsync();

        IEnumerable<TxOutput> produced = await _dbContext.TxOutput
            .Where(txOutput => txOutput.Block == rollbackBlock)
            .ToListAsync();

        IEnumerable<Task> consumeTasks = consumed.ToList().Select(txInput => Task.Run(async () =>
        {
            Address? stakeAddress = TryGetStakeAddress(new Address(txInput.TxOutput.Address));

            if (stakeAddress is not null)
            {
                BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                    .Where((bbsae) => (bbsae.StakeAddress == stakeAddress.ToString()) && (bbsae.Epoch == rollbackBlock.Epoch))
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
            Address? stakeAddress = TryGetStakeAddress(new Address(txOutput.Address));

            if (stakeAddress is not null)
            {
                BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                    .Where((bba) => (bba.StakeAddress == stakeAddress.ToString()) && (bba.Epoch == rollbackBlock.Epoch))
                    .FirstOrDefaultAsync();

                BalanceByStakeAddressEpoch? previousEntry = await GetLastEpochBalanceByStakeAddressAsync(stakeAddress.ToString(), rollbackBlock.Epoch);

                if (entry is not null)
                {
                    entry.Balance -= txOutput.Amount;

                    if (entry.Balance is 0 ||
                        (previousEntry is not null && entry.Balance == previousEntry.Balance))
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