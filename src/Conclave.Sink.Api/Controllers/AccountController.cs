using Blockfrost.Api;
using Conclave.Common.Models;
using Conclave.Common.Responses;
using Conclave.Sink.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly ConclaveSinkDbContext _dbContext;
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountService _accountService;

    public AccountController(ConclaveSinkDbContext dbContext, ILogger<AccountController> logger, IAccountService accountService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accountService = accountService;
    }

    [HttpGet("{stakeAddress}/stakes")]
    public async Task<ActionResult<IEnumerable<BalanceResponse>>> GetStakeBalanceHistory(string stakeAddress, [FromQuery] ulong? fromEpoch, [FromQuery] ulong? toEpoch)
    {
        List<string> dummyConclavePools = new()
        {
            "pool1a7h89sr6ymj9g2a9tm6e6dddghl64tp39pj78f6cah5ewgd4px0",
            "pool1p9xu88dzmpp5l8wmjd6f5xfs9z89mky6up86ty2wz4aavmm8f3m",
            "pool1xe7ey753aw0v79tysaumamkgcugpw9txahxwrrltqjpx25adrkr"
        };

        if (fromEpoch is not null && toEpoch is not null && fromEpoch > toEpoch)
        {
            return BadRequest("fromEpoch must be less than or equal to toEpoch");
        }

        ulong currentEpoch = await _dbContext.Blocks.Select(b => b.Epoch).MaxAsync();


        fromEpoch ??= 0;
        toEpoch = toEpoch is not null ? ulong.Min((ulong)toEpoch, currentEpoch) : currentEpoch;

        var delegations = await _dbContext.StakeDelegations
            .Include(s => s.Transaction)
            .ThenInclude(t => t.Block)
            .Where(s => s.StakeAddress == stakeAddress && s.Transaction.Block.Epoch >= 0 && s.Transaction.Block.Epoch <= toEpoch)
            .GroupBy(s => s.Transaction.Block.Epoch)
            .Select(g => g.OrderByDescending(s => s.Transaction.Block.Slot).First())
            .ToListAsync();

        Dictionary<ulong, bool> conclaveDelegationEpochs = new();

        for (ulong i = (ulong)fromEpoch; i <= toEpoch; i++)
        {
            var lastDelegation = delegations
                    .Where(s => s.Transaction.Block.Epoch <= i)
                    .OrderByDescending(s => s.Transaction.Block.Epoch)
                    .FirstOrDefault();

            conclaveDelegationEpochs.Add(i,
                lastDelegation is not null && dummyConclavePools.Contains(lastDelegation.PoolId)
            );
        }

        // Get Total Rewards
        int page = 1;
        List<EpochReward> rewards = new();
        while (true)
        {
            var rewardsPage = await _accountService.RewardsAsync(stakeAddress, page, 100, ESortOrder.Asc);

            if (rewardsPage is null) break;

            rewards.AddRange(rewardsPage.Select(rp => new EpochReward
            {
                Epoch = (ulong)rp.Epoch,
                Amount = ulong.Parse(rp.Amount)
            }));

            if (rewardsPage.Count < 100) break;

            page++;
        }

        // Get PendingRewards -> total rewards up to a certain epoch - total withdrawals up to a certain epoch
        List<EpochReward> pendingReward = conclaveDelegationEpochs
            .Where(c => c.Value)
            .OrderBy(c => c.Key)
            .Select(c => new EpochReward
            {
                Epoch = c.Key,
                Amount = rewards
                  .Where(r => r.Epoch <= c.Key)
                  .Aggregate(0ul, (acc, r) => acc + r.Amount) -
                  _dbContext.WithdrawalByStakeEpoch
                    .Where(w => w.StakeAddress == stakeAddress && w.Epoch <= c.Key)
                    .OrderByDescending(w => w.Epoch)
                    .Select(w => w.Amount)
                    .FirstOrDefault()
            })
            .ToList();

        IEnumerable<BalanceResponse> balances = conclaveDelegationEpochs
            .Where(c => c.Value)
            .OrderBy(c => c.Key)
            .Select(c => new BalanceResponse()
            {
                Epoch = c.Key,
                Lovelace = _dbContext.BalanceByStakeEpoch
                    .Where(b => b.StakeAddress == stakeAddress && b.Epoch <= c.Key)
                    .OrderByDescending(b => b.Epoch)
                    .Select(b => b.Balance)
                    .FirstOrDefault() +
                    pendingReward
                        .Where(pr => pr.Epoch == c.Key)
                        .Select(pr => pr.Amount)
                        .FirstOrDefault(),
                Conclave = _dbContext.CnclvByStakeEpoch
                    .Where(b => b.StakeAddress == stakeAddress && b.Epoch <= c.Key)
                    .OrderByDescending(b => b.Epoch)
                    .FirstOrDefault()?.Balance ?? 0,
            });

        return Ok(balances);
    }

}
