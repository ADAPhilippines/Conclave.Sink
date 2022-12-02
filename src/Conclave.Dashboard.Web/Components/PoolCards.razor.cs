using Conclave.Dashboard.Web.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Conclave.Dashboard.Web.Components;

public partial class PoolCards
{
  [Parameter]
  public PoolsModel PoolDetails { get; set; } = new();

  private string CardBorder => PoolDetails.IsConclave 
    ? "border-4 mud-border-warning" 
    : "border-4 mud-border-primary";

  private string CardColor => PoolDetails.IsConclave ? "mud-theme-warning" : "mud-theme-primary";

  private Color ButtonColor => PoolDetails.IsConclave ? Color.Tertiary : Color.Secondary;

  private string ButtonBorder => PoolDetails.IsConclave
    ? "border-solid border mud-border-tertiary"
    : "border-solid border mud-border-primary";

  private string IconDisplay => PoolDetails.IsConclave ? "block" : "hidden";

  private Color ProgressBar => PoolDetails.Saturation >= 50 ? Color.Error : Color.Success;

}