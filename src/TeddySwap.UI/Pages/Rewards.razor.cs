using System.Text.Json;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Addresses;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.Response;
using TeddySwap.Common.Services;
using TeddySwap.Common.Utils;
using TeddySwap.UI.Models;
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

    [Inject]
    protected ISnackbar? Snackbar { get; set; }

    protected LeaderBoardResponse LeaderBoardResponse { get; set; } = new LeaderBoardResponse();

    protected decimal TotalRewards => LeaderBoardResponse.BaseReward + TotalItnNftBonus + TotalFisoRewards;

    protected bool IsTestnetRewardsLoaded { get; set; }
    protected bool IsClaimDialogShown { get; set; }
    protected string MainnetAddress { get; set; } = string.Empty;
    protected int TotalRoundOneNft { get; set; }
    protected int TotalRoundTwoNft { get; set; }
    protected decimal TotalRoundOneItnNftBonus { get; set; }
    protected decimal TotalRoundTwoItnNftBonus { get; set; }
    protected decimal TotalItnNftBonus { get; set; }
    protected decimal BaseFisoRewards { get; set; }
    protected decimal TotalFisoRewards { get; set; }

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
        ArgumentNullException.ThrowIfNull(HeartBeatService);

        if (!string.IsNullOrEmpty(CardanoWalletService.ConnectedAddress))
        {
            try
            {
                string[] addresses = await QueryService.Query($"CardanoWalletService.GetUsedAddressesAsync:{CardanoWalletService.SessionId}:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return await CardanoWalletService.GetUsedAddressesAsync();
                });

                PaginatedLeaderBoardResponse response = await QueryService.Query($"/leaderboard/users/addresses/${string.Join(",", addresses)}", async () =>
                {
                    return await SinkService.GetRewardFromAddressesAsync(addresses);
                });

                LeaderBoardResponse = response.Result.FirstOrDefault() ?? LeaderBoardResponse;

                MainnetAddress = await QueryService.Query($"SinkService.GetMainnetAddressFromTestnetAddressAsync:{CardanoWalletService.ConnectedAddress}:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return await SinkService.GetMainnetAddressFromTestnetAddressAsync(CardanoWalletService.ConnectedAddress);
                });

                TotalRoundOneNft = await QueryService.Query($"SinkService.GetNftCountByAddressPolicy:{MainnetAddress}:ab182ed76b669b49ee54a37dee0d0064ad4208a859cc4fdf3f906d87:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return await SinkService.GetNftCountByAddressPolicyAsync(MainnetAddress, "ab182ed76b669b49ee54a37dee0d0064ad4208a859cc4fdf3f906d87");
                });

                TotalRoundTwoNft = await QueryService.Query($"SinkService.GetNftCountByAddressPolicy:{MainnetAddress}:da3562fad43b7759f679970fb4e0ec07ab5bebe5c703043acda07a3c:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return await SinkService.GetNftCountByAddressPolicyAsync(MainnetAddress, "da3562fad43b7759f679970fb4e0ec07ab5bebe5c703043acda07a3c");
                });

                string mainnetStakeAddress = new Address(MainnetAddress).GetStakeAddress().ToString();
                BaseFisoRewards = await QueryService.Query($"SinkService.GetFisoRewardByStakeAddressAsync:{mainnetStakeAddress}:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return (decimal)await SinkService.GetFisoRewardByStakeAddressAsync(new Address(MainnetAddress).GetStakeAddress().ToString());
                });

                TotalFisoRewards = BaseFisoRewards + (BaseFisoRewards * TotalRoundOneNft * 0.05M) + (BaseFisoRewards * TotalRoundTwoNft * 0.02M);
                TotalRoundOneItnNftBonus = TotalRoundOneNft * 5;
                TotalRoundTwoItnNftBonus = TotalRoundTwoNft * 2;
                TotalItnNftBonus = (TotalRoundOneItnNftBonus + TotalRoundTwoItnNftBonus) / 100 * LeaderBoardResponse.BaseReward;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                // @TODO: Push error to analytics
            }
        }

        IsTestnetRewardsLoaded = true;

        await InvokeAsync(StateHasChanged);
    }

    public async void OnClaimClicked()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(CardanoWalletService);
            ArgumentNullException.ThrowIfNull(QueryService);

            IsClaimDialogShown = true;
            await InvokeAsync(StateHasChanged);

        }
        catch
        {
            // @TODO: Push error to analytics
        }
    }

    public async void OnClaimSubmit()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(CardanoWalletService);
            ArgumentNullException.ThrowIfNull(SinkService);
            ArgumentNullException.ThrowIfNull(QueryService);
            ArgumentNullException.ThrowIfNull(Snackbar);

            string[] addresses = await QueryService.Query($"CardanoWalletService.GetUsedAddressesAsync:{CardanoWalletService.SessionId}", async () =>
            {
                return await CardanoWalletService.GetUsedAddressesAsync();
            });

            string messageJson = JsonSerializer.Serialize(new LinkAddressPayload
            {
                MainnetAddress = MainnetAddress,
                TestnetAddresses = addresses
            });

            CardanoSignedMessage signedMessage = await CardanoWalletService.SignMessage(messageJson.ToHex());
            await SinkService.LinkMainnetAddressAsync(await CardanoWalletService.GetStakeAddressAsync(), messageJson.ToHex(), signedMessage);

            await RefreshDataAsync();
            IsClaimDialogShown = false;
            await InvokeAsync(StateHasChanged);
            Snackbar.Add("You have succesfully linked your mainnet address! 🎊", Severity.Success);
        }
        catch
        {
            // @TODO: Push error to analytics
        }
    }

    new public async ValueTask DisposeAsync()
    {
        ArgumentNullException.ThrowIfNull(CardanoWalletService);
        CardanoWalletService.ConnectionStateChange -= OnConnectionStateChanged;
        await base.DisposeAsync();
    }
}