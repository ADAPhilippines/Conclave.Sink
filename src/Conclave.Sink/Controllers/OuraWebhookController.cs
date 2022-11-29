using System.Text.Json;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Reducers;
using Conclave.Sink.Services;
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
    private readonly CardanoService _cardanoService;
    private readonly IEnumerable<IOuraReducer> _reducers;

    public OuraWebhookController(
        ILogger<OuraWebhookController> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IEnumerable<IOuraReducer> reducers
    )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _reducers = reducers;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveEventAsync([FromBody] JsonElement _eventJson)
    {
        OuraEvent? _event = _eventJson.Deserialize<OuraEvent>(ConclaveJsonSerializerOptions);
        if (_event is not null && _event.Context is not null)
        {
            _logger.LogInformation($"Event Received: {_event.Variant}, Block No: {_event.Context.BlockNumber}, Slot No: {_event.Context.Slot}, Block Hash: {_event.Context.BlockHash}");
            await Task.WhenAll(_reducers.Select((reducer) =>
            {
                Task emptyTask = Task.Run(() => { });
                OuraVariant reducerVariant = GetReducerVariant(reducer);
                if (reducerVariant == _event.Variant)
                    return GetReducerVariant(reducer) switch
                    {
                        OuraVariant.Block => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraBlockEvent>(ConclaveJsonSerializerOptions)),
                        OuraVariant.RollBack => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraEvent>(ConclaveJsonSerializerOptions)),
                        OuraVariant.TxInput => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraTxInputEvent>(ConclaveJsonSerializerOptions)),
                        OuraVariant.TxOutput => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraTxOutputEvent>(ConclaveJsonSerializerOptions)),
                        _ => emptyTask
                    };
                else return emptyTask;
            }));
        }
        return Ok();
    }

    private OuraVariant GetReducerVariant(IOuraReducer reducer)
    {
        OuraReducerAttribute? reducerAttribute = reducer.GetType().GetCustomAttributes(typeof(OuraReducerAttribute), true)
            .Where(
                reducerAttributeObject => reducerAttributeObject as OuraReducerAttribute is not null
            )
            .Select(reducerAttributeObject => (reducerAttributeObject as OuraReducerAttribute)).FirstOrDefault();
        return reducerAttribute?.Variant ?? OuraVariant.Unknown;
    }
}