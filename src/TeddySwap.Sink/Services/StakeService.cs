using Blockfrost.Api;
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Services;
using CardanoSharp.Koios.Client;
using TeddySwap.Common.Models.CardanoDbSync;

namespace TeddySwap.Sink.Services;

public class StakeService
{
    private readonly ILogger<StakeService> _logger;
    private readonly IAccountClient _accountClient;
    private readonly IAccountsService _accountsService;
    private readonly ITransactionsService _transactionsService;

    public StakeService(
        ILogger<StakeService> logger,
        IAccountClient accountClient,
        IAccountsService accountsService,
        ITransactionsService transactionsService
        )
    {
        _logger = logger;
        _accountClient = accountClient;
        _accountsService = accountsService;
        _transactionsService = transactionsService;
    }

    public async Task<ulong> GetStakePendingRewardAsync(string stakeAddress)
    {
        return 0;
    }

    public async Task<ulong> GetTotalRewardsByEpochAsync(string stakeAddress, ulong epochNumber)
    {

        return 0;
    }
}