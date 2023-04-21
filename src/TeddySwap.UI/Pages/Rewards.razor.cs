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
    protected bool IsMainnet { get; set; }
    protected string MainnetAddress { get; set; } = string.Empty;
    protected int TotalRoundOneNft { get; set; }
    protected int TotalRoundTwoNft { get; set; }
    protected decimal TotalRoundOneItnNftBonus { get; set; }
    protected decimal TotalRoundTwoItnNftBonus { get; set; }
    protected decimal TotalItnNftBonus { get; set; }
    protected decimal BaseFisoRewards { get; set; }
    protected decimal TotalFisoRewards { get; set; }

    private List<NftDetails> _nfts1 { get; set; } = default!;

    private List<NftDetails> _nfts2 { get; set; } = default!;

    private RewardsBreakdown _rewards { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ArgumentNullException.ThrowIfNull(CardanoWalletService);
        CardanoWalletService.ConnectionStateChange += OnConnectionStateChanged;

        _rewards = new()
        {
            BaseReward = 28_000,
            RoundTwoShare = 21_666.45,
            Bonus = 2_800,
            TotalReward = 52_466.45,
        };

        _nfts1 = new()
        {
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmYQJ2ZbyNCJYcd8xoWP7oMsRK74RpZcPHRo825nNMZHmW",
                Name = "Teddy Bears Club #869",
                RarityRank = "1"
            },
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmQvU95vhKcKhsoEVTw14jJJj7fLfVgf3tF64xWkchSKzP",
                Name = "Teddy Bears Club #909",
                RarityRank = "2"
            },
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmURiqtVSVYa34GQrNAGkMHb4SSGXxkzhs5WzCteomPMtz",
                Name = "Teddy Bears Club #1413",
                RarityRank = "3"
            },
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmNfeeLJzXAGtxbLBfw61UoTG5ZNodGdPbzWoX8vgAw8Mi",
                Name = "Teddy Bears Club #5591",
                RarityRank = "4"
            }
        };

        _nfts2 = new()
        {
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmTgUEKZ9fqTPJyTqeqWzgL1HVjfPWmkGXLtrjZvVepoq6",
                Name = "Teddy Bears Club #2661",
                RarityRank = "1"
            },
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmcrMSZeeyFAV8HnD9MjWkpdev2RoaL3Te1g82qsZhV4hf",
                Name = "Teddy Bears Club #7361",
                RarityRank = "2"
            },
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmVgaJ7ksNsbAzjg3C9eLNUVyxMNqJofKnwb6xRJ83EpDk",
                Name = "Teddy Bears Club #49",
                RarityRank = "3"
            },
            new()
            {
                Image = "https://images.cnft.tools/ipfs/QmeL4bk5f4KoLsTcBFxVFXT82fBjzpUEx7ktAKQVegcnFm",
                Name = "Teddy Bears Club #1454",
                RarityRank = "4"
            }
        };

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
                Address connectedAddress = new Address(CardanoWalletService.ConnectedAddress);
                IsMainnet = connectedAddress.NetworkType == CardanoSharp.Wallet.Enums.NetworkType.Mainnet;

                if (!IsMainnet)
                {
                    string[] addresses = await QueryService.Query($"CardanoWalletService.GetUsedAddressesAsync:{CardanoWalletService.SessionId}:{HeartBeatService.LatestSlotNo}", async () =>
                    {
                        return await CardanoWalletService.GetUsedAddressesAsync();
                    });

                    addresses = addresses.Length <= 0 ? new string[] { CardanoWalletService.ConnectedAddress } : addresses;

                    PaginatedLeaderBoardResponse response = await QueryService.Query($"/leaderboard/users/addresses/${string.Join(",", addresses)}", async () =>
                    {
                        return await SinkService.GetRewardFromAddressesAsync(addresses);
                    });

                    LeaderBoardResponse = response.Result.FirstOrDefault() ?? new LeaderBoardResponse();

                    MainnetAddress = await QueryService.Query($"SinkService.GetMainnetAddressFromTestnetAddressAsync:{CardanoWalletService.ConnectedAddress}:{HeartBeatService.LatestSlotNo}:{MainnetAddress}", async () =>
                    {
                        return await SinkService.GetMainnetAddressFromTestnetAddressAsync(CardanoWalletService.ConnectedAddress);
                    });
                }
                else
                {
                    LeaderBoardResponse = new LeaderBoardResponse();
                }
            }
            catch (Exception ex)
            {
                // @TODO: Push error to analytics
            }

            try
            {
                string queryAddress = IsMainnet ? CardanoWalletService.ConnectedAddress : MainnetAddress;

                TotalRoundOneNft = await QueryService.Query($"SinkService.GetNftCountByAddressPolicy:{CardanoWalletService.SessionId}:{queryAddress}:ab182ed76b669b49ee54a37dee0d0064ad4208a859cc4fdf3f906d87:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return await SinkService.GetNftCountByAddressPolicyAsync(queryAddress, "ab182ed76b669b49ee54a37dee0d0064ad4208a859cc4fdf3f906d87");
                });

                TotalRoundTwoNft = await QueryService.Query($"SinkService.GetNftCountByAddressPolicy:{CardanoWalletService.SessionId}:{queryAddress}:da3562fad43b7759f679970fb4e0ec07ab5bebe5c703043acda07a3c:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return await SinkService.GetNftCountByAddressPolicyAsync(queryAddress, "da3562fad43b7759f679970fb4e0ec07ab5bebe5c703043acda07a3c");
                });

                TotalRoundOneItnNftBonus = TotalRoundOneNft * 5;
                TotalRoundTwoItnNftBonus = TotalRoundTwoNft * 2;
                TotalItnNftBonus = (TotalRoundOneItnNftBonus + TotalRoundTwoItnNftBonus) / 100 * LeaderBoardResponse.BaseReward;
            }
            catch (Exception ex)
            {
                TotalRoundOneNft = 0;
                TotalRoundTwoNft = 0;
                TotalRoundOneItnNftBonus = 0;
                TotalRoundTwoItnNftBonus = 0;
                TotalItnNftBonus = 0;
                // @TODO: Push error to analytics
            }

            try
            {
                string queryAddress = IsMainnet ? CardanoWalletService.ConnectedAddress : MainnetAddress;
                string mainnetStakeAddress = new Address(queryAddress).GetStakeAddress().ToString();
                BaseFisoRewards = await QueryService.Query($"SinkService.GetFisoRewardByStakeAddressAsync:{CardanoWalletService.SessionId}:{mainnetStakeAddress}:{HeartBeatService.LatestSlotNo}", async () =>
                {
                    return (decimal)await SinkService.GetFisoRewardByStakeAddressAsync(mainnetStakeAddress);
                });

                TotalFisoRewards = BaseFisoRewards + (BaseFisoRewards * TotalRoundOneNft * 0.05M) + (BaseFisoRewards * TotalRoundTwoNft * 0.02M);
            }
            catch (Exception ex)
            {
                BaseFisoRewards = 0;
                TotalFisoRewards = 0;
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
            addresses = addresses.Length <= 0 ? new string[] { CardanoWalletService.ConnectedAddress! } : addresses;

            string newMainnetAddress = MainnetAddress;

            string messageJson = JsonSerializer.Serialize(new LinkAddressPayload
            {
                MainnetAddress = newMainnetAddress,
                TestnetAddresses = addresses
            });

            CardanoSignedMessage signedMessage = await CardanoWalletService.SignMessage(messageJson.ToHex());
            await SinkService.LinkMainnetAddressAsync(await CardanoWalletService.GetStakeAddressAsync(), messageJson.ToHex(), signedMessage);

            await RefreshDataAsync();
            IsClaimDialogShown = false;
            await InvokeAsync(StateHasChanged);

            MainnetAddress = newMainnetAddress;
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
