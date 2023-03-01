using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;

namespace TeddySwap.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
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
    public async Task<ActionResult<LeaderboardHistoryResponse>> GetAll()
    {
        var res = await _leaderboardService.FetchAllAsync(null);
        return Ok(res);
    }

}