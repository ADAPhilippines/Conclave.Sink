using System.Text.Json;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet("AddressesByStake/{stakeAddress}")]
    public async Task<IActionResult> GetAddressesByStake(string stakeAddress)
    {
        if(_dbContext.AddressByStake is not null)
            return Ok(await _dbContext.AddressByStake.Where(abs => abs.StakeAddress == stakeAddress).ToListAsync());
        else
            return StatusCode(500);
    }

}