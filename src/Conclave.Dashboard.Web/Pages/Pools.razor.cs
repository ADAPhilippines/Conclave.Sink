using System.ComponentModel;
using Conclave.Dashboard.Web.Models;
using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Pages;

public partial class Pools
{
    [Inject]
    public AppStateService? AppStateService { get; set; }

    [Inject]
    private PoolService PoolService { get; set; } = default!;

    private List<PoolsModel> PoolsList { get; set; } = new();

    public bool IsDarkMode
    {
        get => AppStateService?.IsDarkMode ?? false;
        set
        {
            if (AppStateService is not null) AppStateService.IsDarkMode = value;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (AppStateService is not null)
            AppStateService.PropertyChanged += OnAppStatePropertyChanged;

        PoolsList = await PoolService.GetPoolsListAsync();

        await base.OnInitializedAsync();
    }

    private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }
}