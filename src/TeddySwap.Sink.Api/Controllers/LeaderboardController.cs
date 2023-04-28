using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;
using Asp.Versioning;
using TeddySwap.Common.Enums;

namespace TeddySwap.Sink.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILogger<LeaderboardController> _logger;
    private readonly LeaderboardService _leaderboardService;

    public LeaderboardController(
        ILogger<LeaderboardController> logger,
        LeaderboardService leaderboardService)
    {
        _logger = logger;
        _leaderboardService = leaderboardService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PaginatedLeaderBoardResponse>> GetUserLeaderboardAsync([FromQuery] PaginatedRequest request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _leaderboardService.GetUserLeaderboardAsync(request.Offset, request.Limit);

        return Ok(res);
    }

    [HttpGet("users/address/{address}")]
    public async Task<ActionResult<LeaderBoardResponse>> GetSingleUserAddressLeaderboardAsync(string? address)
    {
        if (string.IsNullOrEmpty(address)) return BadRequest();

        var res = await _leaderboardService.GetSingleUserAddressLeaderboardAsync(address);

        if (res is null) return NotFound();

        return Ok(res);
    }

    [HttpPost("users/addresses")]
    public async Task<ActionResult<LeaderBoardResponse>> GetMultipleAddressUserLeaderboardAsync([FromBody] WalletRewardsRequest request)
    {
        if (request.Addresses is null || request.Addresses.Count < 1) return BadRequest();

        var res = await _leaderboardService.GetMultipleAddressUserLeaderboardAsync(request.Addresses);

        if (res is null) return NotFound();

        return Ok(res);
    }

    [HttpGet("badgers")]
    public async Task<ActionResult<PaginatedLeaderBoardResponse>> GetBadgerLeaderboardAsync([FromQuery] PaginatedRequest request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _leaderboardService.GetBadgerLeaderboardAsync(request.Offset, request.Limit);

        return Ok(res);
    }

    [HttpGet("badgers/address/{address}")]
    public async Task<ActionResult<LeaderBoardResponse>> GetSingleBadgerAddressLeaderboardAsync(string? address)
    {
        if (string.IsNullOrEmpty(address)) return BadRequest();

        var res = await _leaderboardService.GetSingleBadgerAddressLeaderboardAsync(address);

        if (res is null) return NotFound();

        return Ok(res);
    }
}