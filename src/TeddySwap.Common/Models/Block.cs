namespace TeddySwap.Common.Models;

public class Block
{
    public string BlockHash { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;
    public ulong BlockNumber { get; set; }
    public string VrfKeyhash { get; set; } = string.Empty;
    public ulong Slot { get; set; }
    public ulong Epoch { get; set; }
    public IEnumerable<Transaction> Transactions { get; set; } = new List<Transaction>();
    public IEnumerable<ulong>? InvalidTransactions { get; set; }

    public static implicit operator string(Block v)
    {
        throw new NotImplementedException();
    }
}