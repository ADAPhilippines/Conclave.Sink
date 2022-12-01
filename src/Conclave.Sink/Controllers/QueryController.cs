using System.Text.Json;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Conclave.Sink.Reducers;

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
        if (_dbContext.AddressByStake is not null)
            return Ok(await _dbContext.AddressByStake.Where(abs => abs.StakeAddress == stakeAddress).ToListAsync());
        else
            return StatusCode(500);
    }

    [HttpGet("BalanceByStakeAddressEpoch/{stakeAddress}/{epoch}")]
    public async Task<IActionResult> GetBalanceByStakeAddressEpoch(string stakeAddress, ulong epoch)
    {
        BalanceByStakeAddressEpoch? balanceByEpoch = await _dbContext.BalanceByStakeAddressEpoch.Where(s => (s.Epoch <= epoch) && (s.StakeAddress == stakeAddress)).OrderByDescending(s => s.Epoch).FirstOrDefaultAsync();
        return Ok(balanceByEpoch is null ? 0 : balanceByEpoch.Balance);
    }
}