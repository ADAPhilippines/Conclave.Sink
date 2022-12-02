using Conclave.Dashboard.Web.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Conclave.Dashboard.Web.Components;

public partial class PoolCards
{
  [Parameter]
  public bool IsConclavePool { get; set; }

  [Parameter]
  public PoolsModel PoolDetails { get; set; } = new();

  private string CardBorder => PoolDetails.IsConclave switch
  {
    true => "border-4 mud-border-warning",
    false => "border-4 mud-border-primary"
  };

  private string CardColor => PoolDetails.IsConclave switch
  {
    true => "mud-theme-warning",
    false => "mud-theme-primary"
  };

  private Color ButtonColor => PoolDetails.IsConclave switch
  {
    true => Color.Tertiary,
    false => Color.Secondary
  };

  private string ButtonBorder => PoolDetails.IsConclave switch
  {
    true => "border-solid border mud-border-tertiary",
    false => "border-solid border mud-border-primary"
  };

  private string IconDisplay => PoolDetails.IsConclave switch
  {
    true => "block",
    false => "hidden"
  };
}