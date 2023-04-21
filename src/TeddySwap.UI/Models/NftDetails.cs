namespace TeddySwap.UI.Models;

public class NftDetails
{
    public string Image { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double BaseReward { get; set; }

    public double RoundTwoShare { get; set; }

    public double Bonus { get; set; }

    public double TotalReward { get; set; }

    public int Rarity { get; set; }
}
