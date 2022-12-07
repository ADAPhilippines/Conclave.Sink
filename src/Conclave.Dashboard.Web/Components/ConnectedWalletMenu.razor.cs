using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class ConnectedWalletMenu
{
    [Parameter]
    public string AdaBalance { get; set; } = string.Empty;

    [Parameter]
    public string CnclvBalance { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnDisconnectBtnClicked { get; set; }
}
