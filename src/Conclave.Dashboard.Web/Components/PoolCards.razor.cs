using Conclave.Dashboard.Web.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Conclave.Dashboard.Web.Components;

public partial class PoolCards
{
  [Parameter]
  public PoolsModel PoolDetails { get; set; } = new();

  [Parameter]
  public bool IsDarkMode { get; set; }

  private string CardBorder => PoolDetails.IsConclave
    ? "border-4 mud-border-warning"
    : "border-4 mud-border-primary";

  private string CardColor => PoolDetails.IsConclave ? "mud-theme-warning" : "mud-theme-primary";

  private string ButtonTitle => PoolDetails.IsStaked ? "Unstake" : "Stake";

  private string IconDisplay => PoolDetails.IsConclave ? "block" : "hidden";

  private Color ProgressBar => PoolDetails.Saturation >= 50 ? Color.Error : Color.Success;

  private Color ButtonColor()
  {
    return PoolDetails.IsStaked
    ? Color.Error
    : PoolDetails.IsConclave
      ? Color.Tertiary
      : Color.Secondary;
  }

  private string ButtonBorder()
  {
    return PoolDetails.IsStaked
    ? "border-solid border mud-border-error"
    : PoolDetails.IsConclave
      ? "border-solid border mud-border-tertiary"
      : "border-solid border mud-border-primary";
  }

  private string HighlightClass()
  {
    return IsDarkMode ? "hover:bg-card-gradient" : "hover:bg-white";
  }
}