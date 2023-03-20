using Microsoft.AspNetCore.Mvc;
using TeddySwap.Sink.Data;

namespace TeddySwap.Sink.Controllers;

[ApiController]
[Route("[controller]")]
public class QueryController : ControllerBase
{
    private readonly ILogger<QueryController> _logger;
    private readonly TeddySwapSinkCoreDbContext _dbContext;

    public QueryController(
        ILogger<QueryController> logger,
        TeddySwapSinkCoreDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }
}