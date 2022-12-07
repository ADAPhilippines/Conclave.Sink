using System.ComponentModel;
using Conclave.Dashboard.Web.Models;
using Conclave.Dashboard.Web.Services;
using Microsoft.AspNetCore.Components;
using Conclave.Dashboard.Web.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Conclave.Dashboard.Web.Pages;

public partial class Pools
{
  [Inject]
  public AppStateService? AppStateService { get; set; }

  [Inject]
  private PoolService PoolService { get; set; } = default!;

  public string Search { get; set; } = string.Empty;

  private List<PoolsModel> ConclavePoolsList { get; set; } = new();

  private List<PoolsModel> OtherPoolsList { get; set; } = new();

  private List<PoolsModel> PoolsList { get; set; } = new();

  private int ConclavePagination { get; set; }

  private int OtherPoolsPagination { get; set; }

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
    ConclavePoolsList = await PoolService.GetFilteredPoolsListAsync(true);
    OtherPoolsList = await PoolService.GetPaginatedPools(1, 3);

    ConclavePagination = Convert.ToInt32(Decimal.Ceiling((Decimal)ConclavePoolsList.Count / 3));
    OtherPoolsPagination = Convert.ToInt32(Decimal.Ceiling((Decimal)PoolsList.Count / 3));

    await base.OnInitializedAsync();
  }

  private async void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    await InvokeAsync(StateHasChanged);
  }

  private async Task OnKeyPressed(KeyboardEventArgs e)
  {
    if (e.Key == "Enter")
    {
      OtherPoolsList = await PoolService.GetPoolsSearchedList(Search);
    }
  }

  private async Task OnPageChanged(int page)
  {
    OtherPoolsList = await PoolService.GetPaginatedPools(page, 3);
  }
}