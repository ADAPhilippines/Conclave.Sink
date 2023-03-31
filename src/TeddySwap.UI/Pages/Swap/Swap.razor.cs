using MudBlazor;
using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;
using System.Text.Json;

namespace TeddySwap.UI.Pages.Swap;

public partial class Swap
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private double _fromValue { get; set; }

    private double _toValue { get; set; }

    private double? _slippageValue { get; set; } = 1;

    private Token? _fromSelectedToken { get; set; }

    private Token? _toSelectedToken { get; set; }

    private IEnumerable<Token>? Tokens { get; set; } = default!;

    private Token _fromStarterToken { get; set; } = default!;

    private Token _toStarterToken { get; set; } = default!;

    private string _activeTokenChip { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        string tokensJson = File.ReadAllText("./wwwroot/tokens.json");
        ArgumentException.ThrowIfNullOrEmpty(tokensJson);
        Tokens = JsonSerializer.Deserialize<IEnumerable<Token>>(tokensJson);
        if (Tokens is not null)
        {
            _fromStarterToken = Tokens.ElementAt(0);
            _toStarterToken = Tokens.ElementAt(1);
        }
    }

    private void OpenSwapSettingsDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("OnSlippageValueChanged", (double value) => HandleSlippageValueChange(value));
        DialogService.Show<SwapSettingsDialog>("Swap Settings", parameters, options);
    }

    private void OpenSelectTokenDialog(string activeTokenChip)
    {
        _activeTokenChip = activeTokenChip;
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("Tokens", Tokens);
        parameters.Add("OnSelectedTokenClicked", (Token token) => HandleSelectedToken(token));
        DialogService.Show<TokenSelectionDialog>("Select Token", parameters, options);
    }

    private void HandleSelectedToken(Token token)
    {
        if (_activeTokenChip == "fromToken")
        {
            HandleFromSelectedToken(token);
            return;
        }
        HandleToSelectedToken(token);
    }

    private void HandleFromSelectedToken(Token token) => _fromSelectedToken = token;

    private void HandleToSelectedToken(Token token) => _toSelectedToken = token;

    private void HandleSlippageValueChange(double value)
    {
        _slippageValue = value;
        StateHasChanged();
    }
}
