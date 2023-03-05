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
        if (!string.IsNullOrEmpty(CardanoWalletService.ConnectedAddress))
        {
            PaginatedLeaderBoardResponse response = await SinkService.GetLeaderboardAsync(Common.Enums.LeaderBoardType.Users, 0, 1, CardanoWalletService.ConnectedAddress);
            LeaderBoardResponse = response.Result.FirstOrDefault() ?? LeaderBoardResponse;
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