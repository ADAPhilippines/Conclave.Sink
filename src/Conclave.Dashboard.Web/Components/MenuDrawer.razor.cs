using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class MenuDrawer
{
    [Parameter]
    public string WalletAddress { get; set; } = string.Empty;

    [Parameter]
    public bool IsDrawerOpen { get; set; }
}
