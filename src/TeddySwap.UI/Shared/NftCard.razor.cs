using Microsoft.AspNetCore.Components;
using TeddySwap.Common.Models;
using TeddySwap.Common.Models.Response;
using TeddySwap.UI.Models;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Shared;

public partial class NftCard
{
    [Inject]
    protected NftService? NftService { get; set; }

    [Parameter, EditorRequired]
    public AssetResponse NftDetails { get; set; } = new AssetResponse();

    [Parameter, EditorRequired]
    public RewardsBreakdown Rewards { get; set; } = new RewardsBreakdown();

    [Parameter, EditorRequired]
    public string PolicyId { get; set; } = string.Empty;

    public TbcNft? ExtraNftDetails
    {
        get
        {
            ArgumentNullException.ThrowIfNull(NftService);
            return NftService.GetNftAsync(PolicyId, NftDetails.AsciiName);
        }
    }
}
