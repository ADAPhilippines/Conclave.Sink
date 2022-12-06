using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;
using System.ComponentModel;

namespace Conclave.Dashboard.Web.Pages;

public class ConclaveBasePage : ComponentBase, IDisposable
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        AppStateService.IsDrawerOpen = false;
        AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        base.OnInitialized();
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        AppStateService.PropertyChanged -= OnAppStatePropertyChanged;
    }
}
