using Conclave.Common.Models;
using Conclave.Common.Models.Responses;
using Conclave.Sink.Api.Services;
using Microsoft.AspNetCore.Mvc;
namespace Conclave.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly AccountService _accountService;

    public AccountsController(ILogger<AccountsController> logger, AccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [HttpGet("{stakeAddress}/stakes")]
    public async Task<ActionResult<AccountEpochStakesResponse>> GetStakeBalanceHistoryByEpochAsync(string stakeAddress, [FromQuery] ulong? from, [FromQuery] ulong? to)
    {

        if (from is not null && to is not null && from > to)
            return BadRequest(new
            {
                Message = "fromEpoch must be less than or equal to toEpoch"
            });

        var stakes = await _accountService.GetBaseEpochStakes(stakeAddress, from, to);

        return Ok(new
        {
            Message = "ok",
            IsSuccess = true,
            Result = stakes
        });
    }

    [HttpGet("pool/{poolId}/totalStake/{epoch}")]
    public async Task<ActionResult<ConclaveStake>> GetTotalStakes(string poolId, ulong epoch)
    {
        var totalStake = await _accountService.GetTotalPoolStakes(poolId, epoch);

        return Ok(new
        {
            Message = "ok",
            IsSuccess = true,
            Result = totalStake
        });
    }

    [HttpGet("pool/conclave/totalStake/{epoch}")]
    public async Task<ActionResult<ConclaveStake>> GetTotalStakes(ulong epoch)
    {
        var totalStake = await _accountService.GetTotalConclaveStakes(epoch);

        return Ok(new
        {
            Message = "ok",
            IsSuccess = true,
            Result = totalStake
        });
    }

    [HttpGet("{stakeAddress}/rewards")]
    public async Task<ActionResult<IEnumerable<ConclaveEpochStakeRewards>>> GetRewardHistoryAsync(string stakeAddress, [FromQuery] ulong from, [FromQuery] ulong to)
    {

        if (from > to) return BadRequest();

        var rewards = await _accountService.GetConclaveEpochStakeRewards(to);


        return Ok(rewards.Where(r => r.StakeAddress == stakeAddress && r.Epoch >= from && r.Epoch <= to));

    }
}
