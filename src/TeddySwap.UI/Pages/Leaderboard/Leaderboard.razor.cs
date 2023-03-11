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

    protected ulong EndSlot { get => 12128504; }
    protected DateTimeOffset RemainingTime
    {
        get
        {
            DateTimeOffset result = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(EndSlot - HeartBeatService?.LatestSlotNo ?? 0));
            
            if (result < DateTimeOffset.FromUnixTimeSeconds(0))
                result = DateTimeOffset.FromUnixTimeSeconds(0);
                
            return result;
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