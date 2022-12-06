using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Shared;

public partial class Header
{
    [Parameter]
    public EventCallback OnBurgerMenuClicked { get; set; }

    [Parameter]
    public string WalletAddress { get; set; } = string.Empty;

    private bool _isDropDownVisible { get; set; }

    private void OnDropDownClicked() => _isDropDownVisible = !_isDropDownVisible;
}
