using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class TransactionData
{
    public ulong Fee { get; init; }
    public IEnumerable<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}