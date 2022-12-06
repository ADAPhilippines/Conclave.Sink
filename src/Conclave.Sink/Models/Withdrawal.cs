using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record Withdrawal
{
    public string StakeAddress { get; init; }  = string.Empty;
    public ulong Amount { get; init; }
}