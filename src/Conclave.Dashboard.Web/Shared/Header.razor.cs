using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;
using Conclave.Dashboard.Web.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class Header : ConclaveComponentBase
{
    [Parameter]
    public EventCallback OnBurgerMenuClicked { get; set; }

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

    private bool _isConnected { get; set; }

    private bool _isDropDownVisible { get; set; }

    private void ConnectWallet()
    {
        _isConnected = true;
        _isDropDownVisible = false;
    }

    private void ToggleDrawer()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        AppStateService.IsDrawerOpen = !AppStateService.IsDrawerOpen;
        AppStateService.PropertyChanged += OnAppStatePropertyChanged; 
    }

    private void DisconnectWallet() => _isConnected = false;

    private void OnDropDownClicked() => _isDropDownVisible = !_isDropDownVisible;
}
