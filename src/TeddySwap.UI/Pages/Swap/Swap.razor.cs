using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace TeddySwap.UI.Pages.Swap;

public partial class Swap
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private double _fromValue { get; set; }

    private double _toValue { get; set; }

    public string BdayMessage { get; set; } = string.Empty;

    private double? _slippageValue { get; set; } = 1;

    private void OpenDialog()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
        };

        var parameters = new DialogParameters();
        parameters.Add("OnSlippageValueChanged", (double value) => HandleSlippageValueChange(value));
        DialogService.Show<SwapSettingsDialog>("Swap Settings", parameters, options);
    }

    private void HandleSlippageValueChange(double value)
    {
        _slippageValue = value;
        StateHasChanged();
    }
}
