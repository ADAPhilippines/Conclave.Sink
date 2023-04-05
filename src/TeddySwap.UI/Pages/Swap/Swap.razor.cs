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
    protected new AppStateService AppStateService { get; set; } = default!;

    [Inject]
    protected CardanoWalletService? CardanoWalletService { get; set; }

    private IEnumerable<Token>? Tokens { get; set; } = default!;

    private bool _isPanelExpanded { get; set; } = false;

    private bool _areInputsSwapped { get; set; } = false;

    private bool _isChartButtonClicked { get; set; } = false;

    private void ToggleChart() => _isChartButtonClicked = !_isChartButtonClicked;

    private void SwapInputs() => _areInputsSwapped = !_areInputsSwapped;

    private void ToggleExpansionPanel() => _isPanelExpanded = !_isPanelExpanded;

    protected override void OnInitialized()
    { 
        string tokensJson = File.ReadAllText("./wwwroot/tokens.json");
        ArgumentException.ThrowIfNullOrEmpty(tokensJson);
        Tokens = JsonSerializer.Deserialize<IEnumerable<Token>>(tokensJson);

        if (AppStateService is not null)
        {
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
            AppStateService.FromCurrentlySelectedToken = Tokens?.ElementAt(0);
            AppStateService.ToCurrentlySelectedToken = Tokens?.ElementAt(2);
        }
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        => await InvokeAsync(StateHasChanged);
    
    private void OpenSwapSettingsDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        DialogService.Show<SwapSettingsDialog>("Swap Settings",  options);
    }

    private void OpenConfirmSwapDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        DialogService.Show<ConfirmSwapDialog>("Confirm swap", options);
    }
}
