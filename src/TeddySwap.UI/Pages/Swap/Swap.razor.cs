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

    private void ToggleExpansionPanel() => _isPanelExpanded = !_isPanelExpanded;

    protected override void OnInitialized()
    {
        if (AppStateService is not null)
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        
        string tokensJson = File.ReadAllText("./wwwroot/tokens.json");
        ArgumentException.ThrowIfNullOrEmpty(tokensJson);
        Tokens = JsonSerializer.Deserialize<IEnumerable<Token>>(tokensJson);
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        => await InvokeAsync(StateHasChanged);
    
    private void OpenSwapSettingsDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        DialogService.Show<SwapSettingsDialog>("Swap Settings", parameters, options);
    }
}
