using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;
using Conclave.Dashboard.Web.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class MenuDrawer : ConclaveComponentBase
{
    [Inject]
    private ConclaveIconsService ConclaveIcons { get; set; } = new();

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

    public bool IsDarkMode
    {
        get => AppStateService?.IsDarkMode ?? false;
        set
        {
            if (AppStateService is not null) AppStateService.IsDarkMode= value;
        }
    }

    protected override void OnInitialized()
    {
        if (AppStateService is not null)
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        base.OnInitialized();
    }
}
