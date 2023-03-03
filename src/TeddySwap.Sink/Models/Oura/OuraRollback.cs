using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraRollback
{
    [JsonPropertyName("block_slot")]
    public ulong? BlockSlot { get; init; }

    [JsonPropertyName("block_hash")]
    public string? BlockHash { get; init; }
}