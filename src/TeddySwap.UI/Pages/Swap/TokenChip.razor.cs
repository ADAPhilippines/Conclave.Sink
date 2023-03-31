using Microsoft.AspNetCore.Components;

namespace TeddySwap.UI.Pages.Swap;

public partial class TokenChip
{
    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public string Image { get; set; } = string.Empty;

    [Parameter]
    public string Name { get; set; } = string.Empty;
}
