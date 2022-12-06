using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class ThemeSwitch
{
    [Inject]
    private ConclaveIconsService ConclaveIcons { get; set; } = new();

    private bool IsDarkMode { get; set; }
}
