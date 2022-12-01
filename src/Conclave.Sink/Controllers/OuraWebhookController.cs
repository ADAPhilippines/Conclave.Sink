using System.Text.Json;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Reducers;
using Conclave.Sink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            if (_event.Variant == OuraVariant.RollBack)
            {
                OuraRollbackEvent? rollbackEvent = _eventJson.Deserialize<OuraRollbackEvent?>();
                if (rollbackEvent is not null && rollbackEvent.RollBack is not null && rollbackEvent.RollBack.BlockSlot is not null)
                {
                    _logger.LogInformation($"Rollback : Block Slot: {rollbackEvent.RollBack.BlockSlot}, Block Hash: {rollbackEvent.RollBack.BlockHash}");

                    BlockReducer? blockReducer = _reducers.Where(r => r is BlockReducer).FirstOrDefault() as BlockReducer;

                    if (blockReducer is not null)
                        await blockReducer.RollbackAsync((ulong)rollbackEvent.RollBack.BlockSlot);
                }
            }
            else
            {
                _logger.LogInformation($"Event Received: {_event.Variant}, Block No: {_event.Context.BlockNumber}, Slot No: {_event.Context.Slot}, Block Hash: {_event.Context.BlockHash}");
                await Task.WhenAll(_reducers.SelectMany((reducer) =>
                {
                    Task emptyTask = Task.Run(() => { });
                    ICollection<OuraVariant> reducerVariants = _GetReducerVariants(reducer);
                    return reducerVariants.ToList().Select((reducerVariant) =>
                    {
                        if (reducerVariant == _event.Variant)
                        {
                            return reducerVariant switch
                            {
                                OuraVariant.Block => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraBlockEvent>(ConclaveJsonSerializerOptions)),
                                OuraVariant.RollBack => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraEvent>(ConclaveJsonSerializerOptions)),
                                OuraVariant.TxInput => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraTxInputEvent>(ConclaveJsonSerializerOptions)),
                                OuraVariant.TxOutput => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraTxOutputEvent>(ConclaveJsonSerializerOptions)),
                                OuraVariant.PoolRegistration => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraPoolRegistrationEvent>(ConclaveJsonSerializerOptions)),
                                _ => emptyTask
                            };
                        }
                        else return emptyTask;
                    });
                }));
            }
        }
        return Ok();
    }

    private ICollection<OuraVariant> _GetReducerVariants(IOuraReducer reducer)
    {
        OuraReducerAttribute? reducerAttribute = reducer.GetType().GetCustomAttributes(typeof(OuraReducerAttribute), true)
            .Where(
                reducerAttributeObject => reducerAttributeObject as OuraReducerAttribute is not null
            )
            .Select(reducerAttributeObject => (reducerAttributeObject as OuraReducerAttribute)).FirstOrDefault();
        return reducerAttribute?.Variants ?? new OuraVariant[] { OuraVariant.Unknown }.ToList();
    }
}