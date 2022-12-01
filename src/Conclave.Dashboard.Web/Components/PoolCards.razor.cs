using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Conclave.Dashboard.Web.Components;

public partial class PoolCards
{
  [Parameter]
  public bool IsConclavePool { get; set; }
  private string CardBorder => IsConclavePool switch
  {
    true => "border mud-border-warning",
    false => "border mud-border-primary"
  };

  private string CardColor => IsConclavePool switch
  {
    true => "mud-theme-warning",
    false => "mud-theme-primary"
  };

  private Color ButtonColor => IsConclavePool switch
  {
    true => Color.Tertiary,
    false => Color.Secondary
  };

  private string ButtonBorder => IsConclavePool switch
  {
    true => "border-solid border mud-border-tertiary",
    false => "border-solid border mud-border-primary"
  };
}