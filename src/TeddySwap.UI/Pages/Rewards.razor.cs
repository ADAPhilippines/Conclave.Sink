using Microsoft.AspNetCore.Components;
using TeddySwap.Common.Models.Response;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages;

public partial class Rewards : IAsyncDisposable
{
    [Inject]
    protected CardanoWalletService? CardanoWalletService { get; set; }

    [Inject]
    protected SinkService? SinkService { get; set; }

    [Inject]
    protected QueryService? QueryService { get; set; }

    protected LeaderBoardResponse LeaderBoardResponse { get; set; } = new LeaderBoardResponse();

    protected decimal TotalRewards => LeaderBoardResponse.BaseReward;

    protected bool IsTestnetRewardsLoaded { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ArgumentNullException.ThrowIfNull(CardanoWalletService);
        CardanoWalletService.ConnectionStateChange += OnConnectionStateChanged;

        await RefreshDataAsync();
        await base.OnInitializedAsync();
    }

    protected override async void OnHeartBeatEvent(object? sender, EventArgs e)
    {
        await RefreshDataAsync();
    }

    private async void OnConnectionStateChanged(object? sender, EventArgs e)
    {
        await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        IsTestnetRewardsLoaded = false;
        await InvokeAsync(StateHasChanged);

        ArgumentNullException.ThrowIfNull(SinkService);
        ArgumentNullException.ThrowIfNull(CardanoWalletService);
        ArgumentNullException.ThrowIfNull(QueryService);
        if (!string.IsNullOrEmpty(CardanoWalletService.ConnectedAddress))
        {
            try
            {
                string[] addresses = await CardanoWalletService.GetUsedAddressesAsync();
                PaginatedLeaderBoardResponse response = await QueryService.Query($"/leaderboard/users/addresses/${string.Join(",", addresses)}", async () =>
                {
                    return await SinkService.GetRewardFromAddressesAsync(addresses);
                });
                LeaderBoardResponse = response.Result.FirstOrDefault() ?? LeaderBoardResponse;
            }
            catch(Exception ex)
            {
                // @TODO: Push error to analytics
            }
        }

        IsTestnetRewardsLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    new public async ValueTask DisposeAsync()
    {
        ArgumentNullException.ThrowIfNull(CardanoWalletService);
        CardanoWalletService.ConnectionStateChange -= OnConnectionStateChanged;
        await base.DisposeAsync();
    }
}