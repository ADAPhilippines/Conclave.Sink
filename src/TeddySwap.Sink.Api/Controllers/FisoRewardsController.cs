using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;
using Asp.Versioning;

namespace TeddySwap.Sink.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class FisoRewardsController : ControllerBase
{
    private readonly ILogger<FisoRewardsController> _logger;
    private readonly FisoRewardService _fisoRewardService;

    public FisoRewardsController(
        ILogger<FisoRewardsController> logger,
        FisoRewardService fisoRewardService)
    {
        _logger = logger;
        _fisoRewardService = fisoRewardService;
    }

    [HttpGet("address/{stakeAddress}")]
    public async Task<IActionResult> GetFisoRewardBreakdownAsync(string stakeAddress)
    {
        if (string.IsNullOrEmpty(stakeAddress)) return BadRequest();

        var res = await _fisoRewardService.GetFisoRewardBreakdownAsync(stakeAddress);

        return Ok(res);
    }
    [HttpGet("address/test")]
    public async Task<IActionResult> Test()
    {
        decimal res = (decimal)Math.Pow(900000, 0.9) + 100000;

        return Ok(res);
    }
}