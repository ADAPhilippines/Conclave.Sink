using Microsoft.AspNetCore.Components;

namespace TeddySwap.UI.Pages.Swap;

public partial class Tooltip
{
    [Parameter]
    public bool ColonVisible { get; set; } = false;

    [Parameter]
    public string Icon { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
