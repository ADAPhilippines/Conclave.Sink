using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Conclave.Dashboard.Web.Components;

public class ConclaveComponentBase : ComponentBase, IDisposable
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        // AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        base.OnInitialized();
    }

    protected async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        // AppStateService.PropertyChanged -= OnAppStatePropertyChanged;
    }
}
