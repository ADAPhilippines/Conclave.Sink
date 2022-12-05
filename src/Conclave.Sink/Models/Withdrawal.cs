using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public class Withdrawal
{
    [JsonPropertyName("reward_account")]
    public string RewardAccount { get; init; } = string.Empty;
    public ulong Coin { get; init; }
}