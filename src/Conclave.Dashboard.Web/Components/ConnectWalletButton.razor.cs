using Conclave.Dashboard.Web.Services;

namespace Conclave.Dashboard.Web.Components;

public partial class ConnectWalletButton : ConclaveComponentBase
{
    public string WalletAddress { get; set; } = "addr_test1qr...";

    private bool _isDropDownVisible { get; set; }

    public bool IsConnected
    {
        get => AppStateService?.IsConnected ?? false;
        set
        {
            if (AppStateService is not null) AppStateService.IsConnected= value;
        }
    }

    private void ConnectWallet()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        IsConnected = true;
        _isDropDownVisible = false;
        AppStateService.PropertyChanged += OnAppStatePropertyChanged;
    }

    private void DisconnectWallet()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        IsConnected = false;
        AppStateService.PropertyChanged += OnAppStatePropertyChanged;
    }

    private void ToggleDropDownMenu() => _isDropDownVisible = !_isDropDownVisible;
}
