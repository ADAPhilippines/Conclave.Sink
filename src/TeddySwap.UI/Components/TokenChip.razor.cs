using Microsoft.AspNetCore.Components;

namespace TeddySwap.UI.Components;

public partial class TokenChip
{
    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public string Image { get; set; } = string.Empty;

    [Parameter]
    public string Name { get; set; } = string.Empty;
}
