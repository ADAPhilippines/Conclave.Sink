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

    [CascadingParameter]
    public bool IsDarkMode { get; set; }

    private string ConclaveLogo
    {
        get => IsDarkMode ?
            "images/conclave-logo-white.svg" : 
            "images/conclave-logo-dark.svg";
    }

    private string CardanoLogo
    {
        get => IsDarkMode ?
            "images/cardano-white.svg" : 
            "images/cardano-dark.svg";
    }
}
