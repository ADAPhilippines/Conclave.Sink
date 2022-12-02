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

    // [HttpGet("AddressesByStake/{stakeAddress}")]
    // public async Task<IActionResult> GetAddressesByStake(string stakeAddress)
    // {
    //     if (_dbContext.AddressByStake is not null)
    //         return Ok(await _dbContext.AddressByStake.Where(abs => abs.StakeAddress == stakeAddress).ToListAsync());
    //     else
    //         return StatusCode(500);
    // }

    [HttpGet("DelegatorByEpoch/{epoch}/{poolId}")]
    public async Task<IActionResult> GetDelegatorByEpoch(ulong epoch, string poolId)
    {
        if (_dbContext.DelegatorByEpoch is null) return StatusCode(500);

        var delegatorByEpoch = await _dbContext.DelegatorByEpoch.Include(de => de.Block)
            .Where(de => de.Block!.Epoch <= epoch)
            .GroupBy(de => de.StakeAddress)
            .Select(de => de.OrderByDescending(d => d.Block!.Slot).First())
            .ToListAsync();

        var stakeAddressesByPool = delegatorByEpoch.Where(de => de.PoolId == poolId)
            .Select(de => new DelegatorByPool(de.StakeAddress, de.Block!.Epoch))
            .OrderBy(sabp => sabp.SinceEpoch);

        return Ok(stakeAddressesByPool);
    }

    [HttpGet("RewardAddressByPool/{epoch}/{poolId}")]
    public async Task<IActionResult> GetRewardAddressByPoolPerEpoch(ulong epoch, string poolId)
    {
        if (_dbContext.RewardAddressByPoolPerEpoch is null) return StatusCode(500);

        var rewardAddressByPoolPerEpoch = await _dbContext.RewardAddressByPoolPerEpoch.Include(rabppe => rabppe.Block)
            .Where(rabppe => rabppe.Block!.Epoch <= epoch && rabppe.PoolId == poolId)
            .OrderByDescending(rabppe => rabppe.Block!.Slot)
            .FirstOrDefaultAsync();

        if (rewardAddressByPoolPerEpoch is null) return NotFound();

        return Ok(rewardAddressByPoolPerEpoch.RewardAddress);

    }

}