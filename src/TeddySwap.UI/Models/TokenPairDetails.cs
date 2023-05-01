namespace TeddySwap.UI.Models;

public class TokenPairDetails
{
    public List<Token> TokenPair { get; set; } = new();

    public double TVL { get; set; }

    public double Volume24H { get; set; }

    public double Volume7D { get; set; }

    public double Fee { get; set; }

    public double APR { get; set; }

    public int Number { get; set; }

    public bool ShowDetails { get; set; } = false;
}
