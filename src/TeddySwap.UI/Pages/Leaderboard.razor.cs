using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages;

public partial class Leaderboard
{

    protected List<DummyLItem> DummyLeaderboard { get; set; } = new List<DummyLItem>();

    protected async override Task OnInitializedAsync()
    {
        for (int x = 0; x < 10; x++)
        {
            DummyLItem i = new();
            DummyLeaderboard.Add(i);
        }
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    protected void ToggleRowExpand(DummyLItem item)
    {
        item.IsExpandable = !item.IsExpandable;
    }

}