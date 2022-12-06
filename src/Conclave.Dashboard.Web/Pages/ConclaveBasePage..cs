using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Services;

namespace Conclave.Dashboard.Web.Pages;

public class ConclaveBasePage : ComponentBase
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        AppStateService.IsDrawerOpen = false;
        AppStateService.PropertyChanged += async (s, e) => await InvokeAsync(StateHasChanged);
        base.OnInitialized();
    }
}
