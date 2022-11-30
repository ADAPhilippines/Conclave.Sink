using System.ComponentModel;
using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class MainLayout
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    public bool IsDarkMode => AppStateService?.IsDarkMode ?? false;
    public ConclaveTheme Theme { get; set; } = new ConclaveTheme();

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