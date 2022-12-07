using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.OuraEvents;

public class OuraTransaction
{
    public ulong? Fee { get; init; }
    public IEnumerable<OuraWithdrawal>? Withdrawals { get; init; }
}