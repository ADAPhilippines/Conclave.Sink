using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Shared;

public partial class NftCard
{
    [Parameter]
   public NftDetails NftDetails { get; set; } = default!;

    [Parameter]
   public RewardsBreakdown Rewards { get; set; } = default!;
}
