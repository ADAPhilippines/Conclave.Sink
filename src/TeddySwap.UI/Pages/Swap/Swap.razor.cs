using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace TeddySwap.UI.Pages.Swap;

public partial class Swap
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private double _fromValue { get; set; }

    private double _toValue { get; set; }

    private double? _slippageValue { get; set; }

    private void OpenDialog()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
        };
        DialogService.Show<SwapSettingsDialog>("Swap Settings", options);
    }
}
