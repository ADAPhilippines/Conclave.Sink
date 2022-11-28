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
    private readonly ConclaveSinkDbContext _dbContext;
    private readonly JsonSerializerOptions ConclaveJsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public OuraWebhookController(
        ILogger<OuraWebhookController> logger,
        ConclaveSinkDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveBlockAsync([FromBody] JsonElement _eventJson)
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
                        await _ProcessOutputAsync(txOutputEvent);
                    break;
                default:
                    break;
            }
        }
        return Ok();
    }

    private async Task _ProcessOutputAsync(OuraTxOutputEvent txOutputEvent)
    {
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