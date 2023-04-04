using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Services;
using MudBlazor;
using System.ComponentModel;

namespace TeddySwap.UI.Pages.Swap;

public partial class SwapSettingsDialog
{
    [Inject]
    protected new AppStateService AppStateService { get; set; } = default!;

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;

    const double MINIMUM_HONEY_VALUE = 1.2;

    protected override void OnInitialized()
    {   
        if (AppStateService is not null)
        {
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
            AppStateService.HoneyValue = MINIMUM_HONEY_VALUE;
        }

    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private void OnSlippageButtonClicked(int value)
    {
       AppStateService.SlippageToleranceValue = value switch
        {
            1 => 1,
            3 => 3,
            _ => 7
        };
    }

    private void OnMinimumButtonClicked() => AppStateService.HoneyValue = MINIMUM_HONEY_VALUE;
}
