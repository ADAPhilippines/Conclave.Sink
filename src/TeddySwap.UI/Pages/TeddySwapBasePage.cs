using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using TeddySwap.Common.Services;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages;

public class TeddySwapBasePage : ComponentBase, IAsyncDisposable
{
    [Inject]
    protected HeartBeatService? HeartBeatService { get; set; }

    [Inject]
    protected AppStateService? AppStateService { get; set; }

    [Inject]
    protected ILogger<TeddySwapBasePage>? Logger { get; set; }

    private bool IsHeartBeatEventAttached { get; set; }

    protected async override Task OnInitializedAsync()
    {
        Console.WriteLine("initialized");
        ArgumentNullException.ThrowIfNull(HeartBeatService);
        if (!IsHeartBeatEventAttached)
        {
            HeartBeatService.Hearbeat += OnHeartBeatEvent;
            IsHeartBeatEventAttached = true;
        }
    }

    protected virtual void OnHeartBeatEvent(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(HeartBeatService);
        Logger?.LogInformation($"Blockchain Heartbeat Received: {HeartBeatService.LatestBlockNo}");
    }

    public async ValueTask DisposeAsync()
    {
        ArgumentNullException.ThrowIfNull(HeartBeatService);
        HeartBeatService.Hearbeat -= OnHeartBeatEvent;
        IsHeartBeatEventAttached = false;
    }
}
