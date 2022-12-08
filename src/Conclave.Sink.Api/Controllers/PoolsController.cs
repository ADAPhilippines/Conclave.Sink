using Conclave.Common.Models;
using Conclave.Sink.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Conclave.Common.Responses;
using System.Text.Json;
using Conclave.Sink.Api.Parameters;

namespace Conclave.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PoolsController : ControllerBase
{
    private readonly ConclaveSinkDbContext _dbContext;
    private readonly ILogger<PoolsController> _logger;
    public PoolsController(ConclaveSinkDbContext dbContext, ILogger<PoolsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PoolRegistration>>> GetPoolsAsync([FromQuery] GetPoolsParameters parameters)
    {
        IQueryable<string> retiredPoolIds = _dbContext.PoolRetirements.Select(pr => pr.Pool);

        //@TODO: Add Checker for Conclave Pools
        //@TODO: Add Query for Effective Epoch
        //@TODO: Use IQueryAble rather than List
        IQueryable<PoolRegistration> pools = _dbContext.PoolRegistrations
            .Where(p => !retiredPoolIds.Contains(p.PoolId))
            .Where(p => parameters.IsConclave ? false : true);

        if (parameters.Filter is not null)
        {
            pools = pools
                .Where(p => parameters.Filter != null ? p.PoolMetadataJSON != null &&
                    (p.PoolMetadataJSON.RootElement.GetProperty("name").GetString() != null) &&
                    (p.PoolMetadataJSON.RootElement.GetProperty("ticker").GetString() != null) &&
                    (p.PoolMetadataJSON.RootElement.GetProperty("name").GetString()!.Contains(parameters.Filter) ||
                    p.PoolMetadataJSON.RootElement.GetProperty("ticker").GetString()!.Contains(parameters.Filter) ||
                    p.PoolId.Contains(parameters.Filter)) : true);
        }

        int total = await pools.CountAsync();
        // return Ok(pools
        //     .Skip(parameters.PageSize * (parameters.PageNumber - 1)).ToList()
        //     .Take(parameters.PageSize)
        //     .ToList());
    }
}
