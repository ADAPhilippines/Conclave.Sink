using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Conclave.Dashboard.Web.Components;

public partial class PoolCards
{
  [Parameter]
  public bool IsConclavePool { get; set; }
  private string CardBorder { get; set; } = "border mud-border-warning";
  private string CardColor { get; set; } = "mud-theme-warning";
  private Color ButtonColor { get; set; } = Color.Secondary;

  
}