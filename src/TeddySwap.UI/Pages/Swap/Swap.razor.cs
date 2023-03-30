using MudBlazor;
using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Components;
using TeddySwap.UI.Models;
using System.Text.Json;

namespace TeddySwap.UI.Pages.Swap;

public partial class Swap
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private double _fromValue { get; set; }

    private double _toValue { get; set; }

    public string BdayMessage { get; set; } = string.Empty;

    private double? _slippageValue { get; set; } = 1;

    private Token? _selectedToken { get; set; }

    private IEnumerable<Token>? Tokens { get; set; } = default!;

    protected override void OnInitialized()
    {
        string tokensJson = File.ReadAllText("./wwwroot/tokens.json");
        ArgumentException.ThrowIfNullOrEmpty(tokensJson);
        Tokens = JsonSerializer.Deserialize<IEnumerable<Token>>(tokensJson);
    }

    private void OpenSwapSettingsDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("OnSlippageValueChanged", (double value) => HandleSlippageValueChange(value));
        DialogService.Show<SwapSettingsDialog>("Swap Settings", parameters, options);
    }

    private void OpenSelectTokenDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("Tokens", Tokens);
        DialogService.Show<SelectTokenDialog>("Select Token", parameters, options);
    }

    private void HandleSelectedToken(Token token) => _selectedToken = token;
    private void HandleSlippageValueChange(double value)
    {
        _slippageValue = value;
        StateHasChanged();
    }
}
