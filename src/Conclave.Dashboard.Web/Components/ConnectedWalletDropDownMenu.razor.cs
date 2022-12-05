using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class ConnectedWalletDropDownMenu
{
    [Parameter]
    public string AdaBalance { get; set; } = string.Empty;

    [Parameter]
    public string CnclvBalance { get; set; } = string.Empty;
}
