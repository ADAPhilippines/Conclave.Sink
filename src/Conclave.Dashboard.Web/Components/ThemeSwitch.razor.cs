using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class ThemeSwitch : ConclaveComponentBase
{
    [Inject]
    private ConclaveIconsService ConclaveIcons { get; set; } = new();

    [CascadingParameter]
    public bool IsDrawerOpen { get; set; }

    public bool IsDarkMode
    {
        get => AppStateService?.IsDarkMode ?? true;
        set
        {
            if (AppStateService is not null) AppStateService.IsDarkMode = value;
        }
    }

    private void ToggleSwitch()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        IsDarkMode = !IsDarkMode;
        AppStateService.PropertyChanged += OnAppStatePropertyChanged;
    }

    // private string ButtonBorder =>  IsDarkMode ? "border-[#41fbfb]" : "border-[#3d1a6e] bg-white"
}
