using System.Text.Json;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class OuraWebhookController : ControllerBase
{
    private readonly ILogger<OuraWebhookController> _logger;
    private readonly IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly JsonSerializerOptions ConclaveJsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public OuraWebhookController(
        ILogger<OuraWebhookController> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory
    )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveEventAsync([FromBody] JsonElement _eventJson)
    {
        OuraEvent? _event = _eventJson.Deserialize<OuraEvent>(ConclaveJsonSerializerOptions);
        if (_event is not null && _event.Context is not null)
        {
            _logger.LogInformation($"Event Received: {_event.Variant}, Block No: {_event.Context.BlockNumber}, Slot No: {_event.Context.Slot}, Block Hash: {_event.Context.BlockHash}");
            switch (_event.Variant)
            {
                case OuraVariant.TxOutput:
                    OuraTxOutputEvent? txOutputEvent = _eventJson.Deserialize<OuraTxOutputEvent>(ConclaveJsonSerializerOptions);
                    if (txOutputEvent is not null)
                        await Task.WhenAll(
                            _ProcessAddressByStakeAsync(txOutputEvent),
                            _ProcessTxOutputAsync(txOutputEvent),
                            _ProcessProduceBalanceByAddressAsync(txOutputEvent)
                        );
                    break;
                case OuraVariant.TxInput:
                    OuraTxInputEvent? txInputEvent = _eventJson.Deserialize<OuraTxInputEvent>(ConclaveJsonSerializerOptions);
                    if (txInputEvent is not null)
                        await Task.WhenAll(
                            _ProcessConsumeBalanceByAddressAsync(txInputEvent)
                        );
                    break;
                default:
                    break;
            }
        }
        return Ok();
    }

    private async Task _ProcessTxOutputAsync(OuraTxOutputEvent txOutputEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (txOutputEvent.Context is not null &&
            txOutputEvent.TxOutput is not null &&
            txOutputEvent.Context.TxHash is not null &&
            txOutputEvent.Context.OutputIdx is not null &&
            txOutputEvent.TxOutput.Amount is not null &&
            txOutputEvent.TxOutput.Address is not null)
        {
            // This should only happen if there has been a rollback
            if (!await _dbContext.TxOutput
                    .AnyAsync(
                        txOut => txOut.TxHash == txOutputEvent.Context.TxHash &&
                        txOut.Index == (ulong)txOutputEvent.Context.OutputIdx)
                )
            {
                await _dbContext.TxOutput.AddAsync(new()
                {
                    TxHash = txOutputEvent.Context.TxHash,
                    Index = (ulong)txOutputEvent.Context.OutputIdx,
                    Amount = (ulong)txOutputEvent.TxOutput.Amount,
                    Address = txOutputEvent.TxOutput.Address
                });
            }
            else
            {
                _logger.LogWarning($"Duplicate UTXO detected TxHash: {txOutputEvent.Context.TxHash} TxIndex: {txOutputEvent.Context.OutputIdx}");
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task _ProcessAddressByStakeAsync(OuraTxOutputEvent txOutputEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (txOutputEvent is not null && txOutputEvent.TxOutput is not null)
        {
            Address outputAddress = new Address(txOutputEvent.TxOutput.Address);
            Address? stakeAddress = TryGetStakeAddress(outputAddress);

            if (stakeAddress is not null && _dbContext.AddressByStake is not null)
            {
                AddressByStake? entry = await _dbContext.AddressByStake
                    .Where((stakeByAddress) => stakeByAddress.StakeAddress == stakeAddress.ToString())
                    .FirstOrDefaultAsync();

                if (entry is not null)
                {
                    if (!entry.PaymentAddresses.Any(address => address == outputAddress.ToString()))
                        entry.PaymentAddresses.Add(outputAddress.ToString());
                }
                else
                {
                    await _dbContext.AddressByStake.AddAsync(new()
                    {
                        StakeAddress = stakeAddress.ToString(),
                        PaymentAddresses = new List<string>() { outputAddress.ToString() }
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task _ProcessProduceBalanceByAddressAsync(OuraTxOutputEvent txOutputEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
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
    }

    private async Task _ProcessConsumeBalanceByAddressAsync(OuraTxInputEvent txInputEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
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
}