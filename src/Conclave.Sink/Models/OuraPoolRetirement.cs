using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraPoolRetirement : OuraEvent
{
    public string? Pool { get; init; } = string.Empty;
    public ulong? Epoch { get; init; }
}