using Microsoft.AspNetCore.Mvc;

namespace Conclave.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class OuraWebhookController : ControllerBase
{
    private readonly ILogger<OuraWebhookController> _logger;

    public OuraWebhookController(ILogger<OuraWebhookController> logger)
    {
        _logger = logger;
    }
}
