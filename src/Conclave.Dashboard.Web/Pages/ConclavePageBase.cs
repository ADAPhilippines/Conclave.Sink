using Conclave.Dashboard.Web.Services;
using Conclave.Dashboard.Web.Components;

namespace Conclave.Dashboard.Web.Pages;

public class ConclavePageBase : ConclaveComponentBase
{
    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(AppStateService);
        AppStateService.IsDrawerOpen = false;
        base.OnInitialized();
    }
}
