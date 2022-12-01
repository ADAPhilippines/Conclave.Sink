using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput, OuraVariant.TxOutput)]
public class AddressByStakeReducer : OuraReducerBase
{
    private readonly ILogger<AddressByStakeReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    public AddressByStakeReducer(
        ILogger<AddressByStakeReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
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
                    TxOutput? input = await _dbContext.TxOutput
                        .Where(txOut => txOut.TxHash == txInputEvent.TxInput.TxHash && txOut.Index == txInputEvent.TxInput.Index).FirstOrDefaultAsync();

                    if (input is not null)
                    {

                        BalanceByAddress? entry = await _dbContext.BalanceByAddress
                            .Where((bba) => bba.Address == input.Address)
                            .FirstOrDefaultAsync();

                        if (entry is not null)
                        {
                            entry.Balance -= input.Amount;
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
                    txOutputEvent.TxOutput.Amount is not null)
                {
                    Address outputAddress = new Address(txOutputEvent.TxOutput.Address);
                    ulong amount = (ulong)txOutputEvent.TxOutput.Amount;

                    BalanceByAddress? entry = await _dbContext.BalanceByAddress
                        .Where((bba) => bba.Address == outputAddress.ToString())
                        .FirstOrDefaultAsync();

                    if (entry is not null)
                    {
                        entry.Balance += amount;
                    }
                    else
                    {
                        await _dbContext.BalanceByAddress.AddAsync(new()
                        {
                            Address = outputAddress.ToString(),
                            Balance = amount
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }),
            _ => Task.Run(() => { })
        });
    }

    public new async Task RollbackAsync(Block rollbackBlock)
    {
        _logger.LogInformation("AddressByStakeReducer Rollback...");
    }
}