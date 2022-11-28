using System.Text.Json;
using Conclave.Sink.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conclave.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class OuraWebhookController : ControllerBase
{
    private readonly ILogger<OuraWebhookController> _logger;
    private readonly JsonSerializerOptions ConclaveJsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public OuraWebhookController(ILogger<OuraWebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveBlock([FromBody] JsonElement _eventJson)
    {
        OuraEvent? _event = _eventJson.Deserialize<OuraEvent>(ConclaveJsonSerializerOptions);

        if (_event is not null)
        {
            switch (_event.Variant)
            {
                case OuraVariant.TxOutput:
                    OuraTxOutputEvent? txOutputEvent = _eventJson.Deserialize<OuraTxOutputEvent>(ConclaveJsonSerializerOptions);
                    if (txOutputEvent is not null)
                        _ProcessOutput(txOutputEvent);
                    break;
                default:
                    break;
            }
        }
        return Ok();
    }

    private void _ProcessOutput(OuraTxOutputEvent txOutputEvent)
    {
        Console.WriteLine("== PROCESS TxOutput Reducer here  ==");
        Console.WriteLine(JsonSerializer.Serialize(txOutputEvent));
    }
}