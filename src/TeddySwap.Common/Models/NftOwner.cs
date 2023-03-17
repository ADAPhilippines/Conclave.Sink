using System.Runtime.InteropServices;

namespace TeddySwap.Common.Models;

public class NftOwner
{
    public string Address { get; init; } = string.Empty;
    public Nft? Nft { get; init; }
}