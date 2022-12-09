using Microsoft.AspNetCore.Components;

namespace Conclave.Dashboard.Web.Components;

public partial class PoolCardList
{
  [Parameter]
  public RenderFragment? Title { get; set; }

  [Parameter]
  public RenderFragment? SearchBox { get; set; }

  [Parameter]
  public RenderFragment? ChildContent { get; set; }

  [Parameter]
  public int PageCount { get; set; }
}