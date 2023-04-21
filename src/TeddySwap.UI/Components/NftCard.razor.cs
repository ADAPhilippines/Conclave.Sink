using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Components;

public partial class NftCard
{
    [Parameter]
   public NftDetails NftDetails { get; set; } = default!;
}
