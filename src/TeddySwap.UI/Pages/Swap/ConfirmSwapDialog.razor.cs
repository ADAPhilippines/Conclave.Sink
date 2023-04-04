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

    [Inject]
    IDialogService DialogService { get; set; } = default!;

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;

    private void OpenWaitingConfirmationDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        DialogService.Show<WaitingConfirmationDialog>("", options);
    }
}
