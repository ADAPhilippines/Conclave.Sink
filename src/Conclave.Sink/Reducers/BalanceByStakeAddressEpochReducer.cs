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
    private readonly ILogger<AddressByStakeReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private CardanoService _cardanoService;

    public BalanceByStakeAddressEpochReducer(
        ILogger<AddressByStakeReducer> logger,
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
                if (txInputEvent is not null && txInputEvent.TxInput is not null)
                {
                    TxOutput? input = await _dbContext.TxOutput.Include(i => i.Block)
                        .Where(txOut => txOut.TxHash == txInputEvent.TxInput.TxHash && txOut.Index == txInputEvent.TxInput.Index).FirstOrDefaultAsync();

                    if (input is not null && input.Block is not null)
                    {
                        Address outputAddress = new Address(input.Address);
                        Address? stakeAddress = TryGetStakeAddress(outputAddress);

                        if (stakeAddress is null || txInputEvent.Context is null || txInputEvent.Context.Slot is null) return;

                        ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txInputEvent.Context.Slot);

                        BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                            .Where((bbae) => (bbae.StakeAddress == stakeAddress.ToString()) && (bbae.Epoch == epoch))
                            .FirstOrDefaultAsync();

                        if (entry is not null)
                        {
                            entry.Balance -= input.Amount;
                        }
                        else
                        {
                            BalanceByStakeAddressEpoch? lastEpochBalance = await GetLastEpochBalanceByStakeAddress(stakeAddress.ToString(), epoch);

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
                    txOutputEvent.Context is not null)
                {
                    ulong amount = (ulong)txOutputEvent.TxOutput.Amount;
                    Address outputAddress = new Address(txOutputEvent.TxOutput.Address);
                    Address? stakeAddress = TryGetStakeAddress(outputAddress);

                    if (stakeAddress is null || txOutputEvent.Context is null || txOutputEvent.Context.Slot is null) return;

                    ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txOutputEvent.Context.Slot);

                    BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                        .Where((bbae) => (bbae.StakeAddress == stakeAddress.ToString()) && (bbae.Epoch == epoch))
                        .FirstOrDefaultAsync();

                    if (entry is not null)
                    {
                        entry.Balance += amount;
                    }
                    else
                    {
                        BalanceByStakeAddressEpoch? lastEpochBalance = await GetLastEpochBalanceByStakeAddress(stakeAddress.ToString(), epoch);

                        ulong balance = lastEpochBalance is null ? amount : (lastEpochBalance.Balance + amount);

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

    public async Task<BalanceByStakeAddressEpoch?> GetLastEpochBalanceByStakeAddress(string stakeAddress, ulong? epoch)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<BalanceByStakeAddressEpoch> sortedEpochBalances = await _dbContext.BalanceByStakeAddressEpoch
            .Where((bbae) => (bbae.StakeAddress == stakeAddress && bbae.Epoch <= epoch))
            .OrderByDescending(s => s.Epoch).ToListAsync();

        BalanceByStakeAddressEpoch? lastEpoch = sortedEpochBalances.FirstOrDefault();

        return lastEpoch;
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        _logger.LogInformation("BalanceByStakeAddressEpoch Rollback...");
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<TxOutput> txOutputs = await _dbContext.TxOutput.Where(txOut => txOut.Block.BlockHash == rollbackBlock.BlockHash).ToListAsync();
        //TODO:get txInputs with same blockhash as rollback
        foreach (TxOutput txOutput in txOutputs)
        {
            Address outputAddress = new Address(txOutput.Address);
            Address? stakeAddress = TryGetStakeAddress(outputAddress);

            if (stakeAddress is null) continue;

            BalanceByStakeAddressEpoch? entry = await _dbContext.BalanceByStakeAddressEpoch
                .Where(bbsae => (bbsae.StakeAddress == stakeAddress.ToString()) && (bbsae.Epoch == rollbackBlock.Epoch))
                .FirstOrDefaultAsync();

            if (entry is null) continue;

            entry.Balance -= txOutput.Amount;
        }
        
        
        //TODO: create another loop for txInputs
        await _dbContext.SaveChangesAsync();
    }
}