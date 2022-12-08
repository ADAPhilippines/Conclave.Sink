using System.ComponentModel;
using Conclave.Dashboard.Web;
using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class MainLayout
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    private string _walletAddress { get; set; } = "addr_test1qrr86cuspxp7e3cnpcweye...";

    private string _walletAddress2 { get; set; } = "addr_test1qr...";

    public ConclaveTheme Theme { get; set; } = new ConclaveTheme();

    private bool IsDarkMode => AppStateService?.IsDarkMode ?? false;

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        base.OnInitialized();
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e) => await InvokeAsync(StateHasChanged);

    private string BackgroundClass => IsDarkMode ? "block" : "hidden";

    private string LightModeBackgroundClass => IsDarkMode ? "bg-black": "bg-[url(/images/LightVersion.png)]";
}
