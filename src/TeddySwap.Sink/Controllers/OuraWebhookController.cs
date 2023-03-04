using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Reducers;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class OuraWebhookController : ControllerBase
{
    private readonly ILogger<OuraWebhookController> _logger;
    private readonly IDbContextFactory<TeddySwapSinkDbContext> _dbContextFactory;
    private readonly JsonSerializerOptions ConclaveJsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly CardanoService _cardanoService;
    private readonly IEnumerable<IOuraReducer> _reducers;
    private readonly IOptions<TeddySwapSinkSettings> _settings;
    public OuraWebhookController(
        ILogger<OuraWebhookController> logger,
        IDbContextFactory<TeddySwapSinkDbContext> dbContextFactory,
        CardanoService cardanoService,
        IEnumerable<IOuraReducer> reducers,
        IOptions<TeddySwapSinkSettings> settings
    )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _reducers = reducers;
        _settings = settings;
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
                        await blockReducer.RollbackBySlotAsync((ulong)rollbackEvent.RollBack.BlockSlot);
                }
            }
            else
            {
                _logger.LogInformation($"Event Received: {_event.Variant}, Block No: {_event.Context.BlockNumber}, Slot No: {_event.Context.Slot}, Block Hash: {_event.Context.BlockHash}");
                await Task.WhenAll(_reducers.SelectMany((reducer) =>
                {
                    ICollection<OuraVariant> reducerVariants = _GetReducerVariants(reducer);
                    return reducerVariants.ToList().Select((reducerVariant) =>
                    {
                        if (reducerVariant == _event.Variant &&
                            (
                                _settings.Value.Reducers.Any(rS => reducer.GetType().FullName?.Contains(rS) ?? false) ||
                                reducer is IOuraCoreReducer
                            )
                        )
                        {
                            return reducerVariant switch
                            {
                                OuraVariant.Block => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraBlockEvent>(ConclaveJsonSerializerOptions)),
                                OuraVariant.Transaction => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraTransactionEvent>(ConclaveJsonSerializerOptions)),
                                OuraVariant.TxOutput => reducer.HandleReduceAsync(_eventJson.Deserialize<OuraTxOutputEvent>(ConclaveJsonSerializerOptions)),
                                _ => Task.CompletedTask
                            };
                        }
                        else return Task.CompletedTask;
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