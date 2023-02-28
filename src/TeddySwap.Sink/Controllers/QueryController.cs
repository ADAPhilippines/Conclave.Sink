using TeddySwap.Sink.Data;
using Microsoft.AspNetCore.Mvc;

namespace Conclave.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class QueryController : ControllerBase
{
    private readonly ILogger<QueryController> _logger;
    private readonly ConclaveSinkDbContext _dbContext;

    public QueryController(
        ILogger<QueryController> logger,
        ConclaveSinkDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }
}