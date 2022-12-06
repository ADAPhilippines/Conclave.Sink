using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;
using System.ComponentModel;

namespace Conclave.Dashboard.Web.Shared;

public partial class MenuDrawer
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    [Parameter]
    public string WalletAddress { get; set; } = string.Empty;

    public bool IsDrawerOpen
    {
        get => AppStateService?.IsDrawerOpen ?? false;
        set
        {
            if (AppStateService is not null) AppStateService.IsDrawerOpen= value;
        }
    }

    protected override void OnInitialized()
    {
        if (AppStateService is not null)
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        base.OnInitialized();
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }
}
