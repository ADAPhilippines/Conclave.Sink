using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TeddySwap.UI.Pages.Swap;

public partial class WaitingConfirmationDialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;
}
