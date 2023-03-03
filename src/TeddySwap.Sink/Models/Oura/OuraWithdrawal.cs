using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraWithdrawal
{
    [JsonPropertyName("reward_account")]
    public string? RewardAccount { get; init; }
    public ulong? Coin { get; init; }
}