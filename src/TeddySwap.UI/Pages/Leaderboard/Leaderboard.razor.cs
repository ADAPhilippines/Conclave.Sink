using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeddySwap.Common.Enums;
using TeddySwap.Common.Models.Response;
using TeddySwap.UI.Models;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages.Leaderboard;

public partial class Leaderboard
{
    protected LeaderboardTable? LeaderBoardTable { get; set; }

    protected ulong EndSlot { get => 12142438; }
    protected TimeSpan RemainingTime
    {
        get
        {
            if (HeartBeatService?.LatestSlotNo > EndSlot) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(Convert.ToDouble(EndSlot - HeartBeatService?.LatestSlotNo));
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async void OnHeartBeatEvent(object? s, EventArgs e)
    {
        try
        {
            await InvokeAsync(async () =>
            {
                if (LeaderBoardTable is not null)
                    await LeaderBoardTable.RefreshDataAsync();
                StateHasChanged();
            });
        }
        catch
        {
            // @TODO log error
        }
    }

}
