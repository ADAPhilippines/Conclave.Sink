using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;
using Conclave.Dashboard.Web.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class Header : ConclaveComponentBase
{
    [Inject]
    private ConclaveIconsService ConclaveIcons { get; set; } = new();

    [CascadingParameter]
    public bool IsDarkMode { get; set; }

    [Parameter]
    public EventCallback OnBurgerMenuClicked { get; set; }

    [Parameter]
    public string WalletAddress { get; set; } = string.Empty;

    public bool IsDrawerOpen
    {
        get => AppStateService?.IsDrawerOpen ?? false;
        set
        {
            if (AppStateService is not null) AppStateService.IsDrawerOpen = value;
        }
    }

    private string ConclaveLogo
    {
        get => IsDarkMode ?
            "images/conclave-logo-white.svg" : 
            "images/conclave-logo-dark.svg";
    }

    private string ConclaveText
    {
        get => IsDarkMode ?
            "images/conclave-text-white.svg" : 
            "images/conclave-text-dark.svg";
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
        IsDrawerOpen = !IsDrawerOpen;
        AppStateService.PropertyChanged += OnAppStatePropertyChanged; 
    }

    private void DisconnectWallet() => _isConnected = false;

    private void OnDropDownClicked() => _isDropDownVisible = !_isDropDownVisible;
}
