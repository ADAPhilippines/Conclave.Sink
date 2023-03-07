using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeddySwap.Common.Enums;
using TeddySwap.Common.Models.Response;
using TeddySwap.UI.Models;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages.Leaderboard;

public partial class LeaderboardTable
{
    [Inject]
    protected SinkService? SinkService { get; set; }

    [Inject]
    protected QueryService? QueryService { get; set; }

    protected IEnumerable<LeaderBoardItem>? LeaderBoardItems { get; set; }
    protected MudTable<LeaderBoardItem>? LeaderBoardTable { get; set; }
    protected string SearchQuery { get; set; } = string.Empty;
    protected PaginatedLeaderBoardResponse LeaderBoardStats { get; set; } = new();

    [Parameter]
    public LeaderBoardType LeaderBoardType { get; set; } = LeaderBoardType.Users;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RefreshDataAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            await InvokeAsync(async () =>
            {
                ArgumentNullException.ThrowIfNull(SinkService);
                ArgumentNullException.ThrowIfNull(QueryService);

                if (LeaderBoardTable is not null)
                    await LeaderBoardTable.ReloadServerData();

                LeaderBoardStats = await QueryService.Query($"/leaderboard/{LeaderBoardType}/0/0", async () =>
                {
                    return await SinkService.GetLeaderboardAsync(LeaderBoardType, 0, 0);
                });
                
                await InvokeAsync(StateHasChanged);
            });
        }
        catch
        {
            // @TODO log the error
        }
    }

    protected async Task<TableData<LeaderBoardItem>> LeaderboardServerData(TableState ts)
    {
        TableData<LeaderBoardItem> tableData = new TableData<LeaderBoardItem>();
        if (SinkService is not null)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(QueryService);

                PaginatedLeaderBoardResponse resp = await QueryService.Query($"/leaderboard/{LeaderBoardType}/{ts.Page * ts.PageSize}/{ts.PageSize}/{SearchQuery}", async () =>
                {
                    return await SinkService.GetLeaderboardAsync(LeaderBoardType, ts.Page * ts.PageSize, ts.PageSize, SearchQuery);
                });

                IEnumerable<LeaderBoardItem> result = resp.Result.Select(lbr => LeaderBoardItem.FromResponse(lbr)).Where(lbr => lbr is not null) as IEnumerable<LeaderBoardItem>;
                if (result is not null)
                {
                    tableData.Items = result;
                    tableData.TotalItems = resp?.TotalCount ?? 0;
                }
                else
                {
                    tableData.Items = new List<LeaderBoardItem>();
                    tableData.TotalItems = 0;
                }
            }
            catch
            {
                // @TODO: Push error to analytics
                tableData.Items = new List<LeaderBoardItem>();
                tableData.TotalItems = 0;
            }
        }
        return tableData;
    }

    protected async Task OnSearch(string value)
    {
        SearchQuery = value;
        await RefreshDataAsync();
    }

    protected void ToggleRowExpand(LeaderBoardItem item)
    {
        item.IsExpanded = !item.IsExpanded;
    }

}