using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeddySwap.UI.Services;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Pages.Swap;

public partial class SwapChart
{
    [Parameter]
    public Token FromCurrentlySelectedToken { get; set; } = new();

    [Parameter]
    public Token ToCurrentlySelectedToken { get; set; } = new();


    private int Index = -1;

    public bool _areTokensSwapped { get; set; } = false;

    public List<ChartSeries> Series = new List<ChartSeries>()
    {
        new ChartSeries() { Name = "Series 1", Data = new double[] { 90, 79, 72, 69, 62, 62, 55, 65, 70 } },
        new ChartSeries() { Name = "Series 2", Data = new double[] { 10, 41, 35, 51, 49, 62, 69, 91, 148 } },
    };
    public string[] XAxisLabels = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };

    Random random = new Random();
    public void RandomizeData()
    {
        var new_series = new List<ChartSeries>()
        {
            new ChartSeries() { Name = "Series 1", Data = new double[9] },
            new ChartSeries() { Name = "Series 2", Data = new double[9] },
        };
        for (int i = 0; i < 9; i++)
        {
            new_series[0].Data[i] = random.NextDouble() * 100;
            new_series[1].Data[i] = random.NextDouble() * 100;
        }
        Series = new_series;
        StateHasChanged();
    }

    private void SwapTokens() => _areTokensSwapped = !_areTokensSwapped;
}
