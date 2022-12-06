using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraWithdrawal
{
    [JsonPropertyName("reward_account")]
    public string? RewardAccount { get; init; } 
    public ulong? Coin { get; init; }
}