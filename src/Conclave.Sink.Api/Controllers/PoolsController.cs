using Conclave.Common.Models;
using Conclave.Common.Parameters;
using Conclave.Sink.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PoolsController : ControllerBase
{
    private readonly ConclaveSinkDbContext _dbContext;
    public PoolsController(ConclaveSinkDbContext dbContext, ILogger<PoolsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private readonly ILogger<PoolsController> _logger;

    [HttpGet]
    public async Task<ActionResult<List<PoolRegistration>>> GetPools([FromQuery] GetPoolsParameters parameters)
    {
        IQueryable<PoolRetirement> retiredPools = _dbContext.PoolRetirements;

        //@TODO: Add logic for stakeDelegation
        IQueryable<PoolRegistration> pools = _dbContext.PoolRegistrations
            .Include(p => p.Transaction)
            .ThenInclude(t => t.Block)
            .Where(p => retiredPools.Where(rp => rp.Pool == p.PoolId).FirstOrDefault() != null)
            .GroupBy(p => p.PoolId, (poolId,pools) => pools.OrderByDescending(e => e.Transaction.Block.Epoch).First())
            .Where(p => parameters.IsConclave ? false : true);
        
        if (parameters.Filter is not null)
        {
            pools = pools
                .Where(p => p.PoolMetadataJSON != null &&  
                    !String.IsNullOrEmpty(p.PoolMetadataJSON.RootElement.GetProperty("name").GetString()) && 
                    !String.IsNullOrEmpty(p.PoolMetadataJSON.RootElement.GetProperty("ticker").GetString()) && 
                    (p.PoolMetadataJSON.RootElement.GetProperty("name").GetString()!.Contains(parameters.Filter) ||
                    p.PoolMetadataJSON.RootElement.GetProperty("ticker").GetString()!.Contains(parameters.Filter)) ||
                    p.PoolId.Contains(parameters.Filter));
        }

        int totalCount = await pools.CountAsync();

        return Ok(pools.Skip(parameters.Offset).Take(parameters.Limit));
    }

    public async Task<int> GetMintedBlocksByVrfKey(string vrfKeyHash) => await _dbContext.Blocks.Where(b => b.VrfKeyhash == vrfKeyHash).CountAsync();
}
