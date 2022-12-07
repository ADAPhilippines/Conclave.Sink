using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class ThemeSwitch
{
    [Inject]
    private ConclaveIconsService ConclaveIcons { get; set; } = new();

    private bool _isDarkMode { get; set; }

    private bool IsDarkMode { get; set; }

    private void ToggleSwitch() => _isDarkMode = !_isDarkMode;
}
