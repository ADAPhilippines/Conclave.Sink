using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class OuraTransaction
{
    public ulong Fee { get; init; }
    public IEnumerable<OuraWithdrawal> Withdrawals { get; set; } = new List<OuraWithdrawal>();
}