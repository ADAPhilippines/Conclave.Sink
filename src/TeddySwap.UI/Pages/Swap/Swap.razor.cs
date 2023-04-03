using MudBlazor;
using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Services;
using TeddySwap.UI.Models;
using System.Text.Json;
using System.ComponentModel;

namespace TeddySwap.UI.Pages.Swap;

public partial class Swap
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    public AppStateService? AppStateService { get; set; }

    [Inject]
    protected CardanoWalletService? CardanoWalletService { get; set; }

    public double SlippageToleranceValue
    {
        get => AppStateService?.SlippageToleranceValue ?? 3;
        set
        {
            if (AppStateService is not null) AppStateService.SlippageToleranceValue = value;
        }
    }

    private double _fromValue { get; set; }

    private double _toValue { get; set; }

    private IEnumerable<Token>? Tokens { get; set; } = default!;

    private bool _isPanelExpanded { get; set; } = false;

    private void ToggleExpansionPanel() => _isPanelExpanded = !_isPanelExpanded;

    
    protected override void OnInitialized()
    {
        if (AppStateService is not null)
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        
        string tokensJson = File.ReadAllText("./wwwroot/tokens.json");
        ArgumentException.ThrowIfNullOrEmpty(tokensJson);
        Tokens = JsonSerializer.Deserialize<IEnumerable<Token>>(tokensJson);
        base.OnInitialized();
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }
    
    private void OpenSwapSettingsDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("OnSlippageValueChanged", (double value) => HandleSlippageValueChange(value));
        DialogService.Show<SwapSettingsDialog>("Swap Settings", parameters, options);
    }

    private void HandleSlippageValueChange(double value) => SlippageToleranceValue = value;
}
