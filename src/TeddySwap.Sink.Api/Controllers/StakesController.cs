using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;
using Asp.Versioning;

namespace TeddySwap.Sink.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class StakesController : ControllerBase
{
    private readonly ILogger<StakesController> _logger;
    private readonly StakeService _stakeService;

    public StakesController(
        ILogger<StakesController> logger,
        StakeService stakeService)
    {
        _logger = logger;
        _stakeService = stakeService;
    }

    [HttpGet("pool/{poolId}/latest")]
    public async Task<IActionResult> GetPoolLiveStakeAsync([FromRoute] string poolId)
    {
        if (string.IsNullOrEmpty(poolId)) return BadRequest();

        var res = await _stakeService.GetPoolLiveStakeAsync(poolId);

        return Ok(res);
    }

    [HttpGet("pool/{poolId}/block/{blockNumber}")]
    public async Task<IActionResult> GetPoolLiveStakeByBlockAsync(string poolId, int blockNumber)
    {
        if (string.IsNullOrEmpty(poolId)) return BadRequest();

        var res = await _stakeService.GetPoolLiveStakeByBlockAsync(poolId, blockNumber);

        return Ok(res);
    }

    [HttpGet("pool/{poolId}/fromBlock/{fromBlock}/toBlock/{toBlock}")]
    public async Task<IActionResult> GetPoolLiveStakeByBlockAsync(string poolId, int fromBlock, int toBlock)
    {
        if (string.IsNullOrEmpty(poolId)) return BadRequest();

        var res = await _stakeService.GetPoolLiveStakeTotalsByBlockAsync(poolId, Enumerable.Range(fromBlock, toBlock).ToList());

        return Ok(res);
    }

    [HttpGet("pool/{poolId}/startBlock/{startBlockNumber}/endBlock/{endBlockNumber}/offsetBaseStake/{offsetBaseStake}")]
    public async Task<IActionResult> GetPoolLiveStakeDeltaByBlockAsync(string poolId, int startBlockNumber, int endBlockNumber, decimal offsetBaseStake)
    {
        if (string.IsNullOrEmpty(poolId)) return BadRequest();

        var res = await _stakeService.GetPoolLiveStakeDeltaByBlockAsync(poolId, startBlockNumber, endBlockNumber, offsetBaseStake);

        return Ok(res);
    }

    [HttpGet("pool/{poolId}/delegators/latest")]
    public async Task<IActionResult> GetPoolDelegatorsAsync([FromRoute] string poolId)
    {
        if (string.IsNullOrEmpty(poolId)) return BadRequest();

        var res = await _stakeService.GetPoolDelegatorsAsync(poolId);

        return Ok(res);
    }

    [HttpGet("stake/{stakeAddress}/latest")]
    public async Task<IActionResult> GetStakeAddressLiveStakeAsync([FromRoute] string stakeAddress)
    {
        if (string.IsNullOrEmpty(stakeAddress)) return BadRequest();

        var res = await _stakeService.GetStakeAddressLiveStakeAsync(stakeAddress);

        return Ok(res);
    }

    [HttpGet("stake/{stakeAddress}/block/{blockNumber}")]
    public async Task<IActionResult> GetStakeAddressLiveStakeByBlockAsync([FromRoute] string stakeAddress, int blockNumber)
    {
        if (string.IsNullOrEmpty(stakeAddress)) return BadRequest();

        var res = await _stakeService.GetStakeAddressLiveStakeByBlockAsync(stakeAddress, blockNumber);

        return Ok(res);
    }
}