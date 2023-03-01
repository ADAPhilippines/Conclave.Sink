using Microsoft.AspNetCore.Mvc;

namespace TeddySwap.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(ILogger<LeaderboardController> logger)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [HttpGet("{stakeAddress}/stakes")]
    public async Task<ActionResult<AccountEpochStakesResponse>> GetStakeBalanceHistoryByEpochAsync()
    {


        return Ok();
    }


}