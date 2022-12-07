using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;
using Conclave.Dashboard.Web.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class MenuDrawer : ConclaveComponentBase
{
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
}
