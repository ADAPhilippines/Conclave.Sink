using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class Transaction
{
    public ulong Fee { get; init; }
    public IEnumerable<Withdrawal> Withdrawals { get; init; } = new List<Withdrawal>();
}