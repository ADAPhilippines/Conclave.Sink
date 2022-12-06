
using System.Text.Json;

namespace Conclave.Sink.Models;

public class PoolRetirement
{
    public string Pool { get; init; } = string.Empty;
    public ulong EffectiveEpoch { get; init; }
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
}