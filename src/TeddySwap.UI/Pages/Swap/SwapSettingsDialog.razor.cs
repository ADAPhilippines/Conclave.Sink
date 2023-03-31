using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TeddySwap.UI.Pages.Swap;

public partial class SwapSettingsDialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public Action<double> OnSlippageValueChanged { get; set; } = default!;

    private double? _slippageValue { get; set; } = 7;

    private double? _minValue { get; set; } = 1.2;

    private string _slippageBtnClass = string.Empty;

    private void OnSlippageButtonClicked(int id)
    {
        _slippageValue = id switch
        {
            1 => 1,
            3 => 3,
            _ => 7
        };
    }

    private void OnMinimumButtonClicked() => _minValue = 1.2;
}
