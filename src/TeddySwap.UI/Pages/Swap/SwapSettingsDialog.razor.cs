using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Services;
using MudBlazor;
using System.ComponentModel;

namespace TeddySwap.UI.Pages.Swap;

public partial class SwapSettingsDialog
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public Action<double> OnSlippageValueChanged { get; set; } = default!;

    public double SlippageToleranceValue
    {
        get => AppStateService?.SlippageToleranceValue ?? 3;
        set
        {
            if (AppStateService is not null) AppStateService.SlippageToleranceValue = value;
        }
    }

    private double? _minValue { get; set; } = 1.2;


    protected override void OnInitialized()
    {   
         if (AppStateService is not null)
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;
        base.OnInitialized();
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private void OnSlippageButtonClicked(int value)
    {
       SlippageToleranceValue = value switch
        {
            1 => 1,
            3 => 3,
            _ => 7
        };
    }

    private void OnMinimumButtonClicked() => _minValue = 1.2;
}
