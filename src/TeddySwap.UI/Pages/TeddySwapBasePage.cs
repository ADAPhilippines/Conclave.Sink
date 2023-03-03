using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages;

public class TeddySwapBasePage : ComponentBase, IAsyncDisposable
{
    [Inject]
    protected HeartBeatService? HeartBeatService { get; set; }

    private bool IsHeartBeatEventAttached { get; set; }

    protected async override Task OnInitializedAsync()
    {
        if (HeartBeatService is not null && !IsHeartBeatEventAttached)
        {
            HeartBeatService.Hearbeat += OnHeartBeatEvent;
            IsHeartBeatEventAttached = true;
        }
    }

    protected virtual void OnHeartBeatEvent(object? sender, EventArgs e)
    {
        if (HeartBeatService is not null)
        {
            Console.WriteLine($"Blockchain Heartbeat Received: {HeartBeatService.LatestBlockNo}");
        }
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (HeartBeatService is not null)
        {
            HeartBeatService.Hearbeat -= OnHeartBeatEvent;
            IsHeartBeatEventAttached = false;
        }
    }
}