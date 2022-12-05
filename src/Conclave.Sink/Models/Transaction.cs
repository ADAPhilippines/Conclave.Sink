using System.Text.Json;

namespace Conclave.Sink.Models;

public class Transaction
{
    public string Hash { get; init; } = string.Empty;
    public ulong Fee { get; init; }
    public IEnumerable<Withdrawal>? Withdrawals { get; init; }
    public Block Block { get; init; } = new();
}