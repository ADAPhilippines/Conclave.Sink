using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;

namespace TeddySwap.Sink.Api.Controllers;

[ApiController]
[Route("leaderboard")]
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

    [HttpGet]
    public async Task<ActionResult<PaginatedLeaderboardResponse>> GetLeaderboardAsync([FromQuery] PaginatedRequestBase request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _leaderboardService.GetLeaderboardAsync(request.Offset, request.Limit);

        return Ok(res);
    }

    [HttpGet("address/{address}")]
    public async Task<ActionResult<LeaderboardResponse>> GetLeaderboardAddressAsync(string? address)
    {
        if (string.IsNullOrEmpty(address)) return BadRequest();

        var res = await _leaderboardService.GetLeaderboardAddressAsync(address);

        if (res is null) return NotFound();

        return Ok(res);
    }

    [HttpGet("user")]
    public async Task<ActionResult<PaginatedLeaderboardResponse>> GetUserLeaderboardAsync([FromQuery] PaginatedRequestBase request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _leaderboardService.GetUserLeaderboardAsync(request.Offset, request.Limit);

        return Ok(res);
    }

    [HttpGet("user/address/{address}")]
    public async Task<ActionResult<LeaderboardResponse>> GetUserLeaderboardAddressAsync(string? address)
    {
        if (string.IsNullOrEmpty(address)) return BadRequest();

        var res = await _leaderboardService.GetUserLeaderboardAddressAsync(address);

        if (res is null) return NotFound();

        return Ok(res);
    }

    [HttpGet("batcher")]
    public async Task<ActionResult<PaginatedLeaderboardResponse>> GetBatcherLeaderboardAsync([FromQuery] PaginatedRequestBase request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _leaderboardService.GetBatcherLeaderboardAsync(request.Offset, request.Limit);

        return Ok(res);
    }

    [HttpGet("batcher/address/{address}")]
    public async Task<ActionResult<LeaderboardResponse>> GetBatcherLeaderboardAddressAsync(string? address)
    {
        if (string.IsNullOrEmpty(address)) return BadRequest();

        var res = await _leaderboardService.GetBatcherLeaderboardAddressAsync(address);

        if (res is null) return NotFound();

        return Ok(res);
    }

}