using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.OuraEvents;

public record OuraWithdrawal
{
    [JsonPropertyName("reward_account")]
    public string? RewardAccount { get; init; } 
    public ulong? Coin { get; init; }
}