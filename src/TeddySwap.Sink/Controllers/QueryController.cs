using TeddySwap.Sink.Data;
using Microsoft.AspNetCore.Mvc;

namespace TeddySwap.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class QueryController : ControllerBase
{
    private readonly ILogger<QueryController> _logger;
    private readonly TeddySwapSinkDbContext _dbContext;

    public QueryController(
        ILogger<QueryController> logger,
        TeddySwapSinkDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }
}