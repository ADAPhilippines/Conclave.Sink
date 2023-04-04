using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Services;
using MudBlazor;

namespace TeddySwap.UI.Pages.Swap;

public partial class ConfirmSwapDialog
{
    [Inject]
    public AppStateService AppStateService { get; set; } = default!;

    [Inject]
    public IconsService IconsService { get; set; } = default!;
}
