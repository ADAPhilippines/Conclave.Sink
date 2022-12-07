using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraContext
{
    [JsonPropertyName("block_hash")]
    public string? BlockHash { get; init; }

    [JsonPropertyName("block_number")]
    public ulong? BlockNumber { get; init; }
    
    public ulong? Slot { get; init; }
    public ulong? Timestamp { get; init; }

    [JsonPropertyName("tx_idx")]
    public ulong? TxIdx { get; init; }

    [JsonPropertyName("tx_hash")]
    public string? TxHash { get; init; }

    [JsonPropertyName("input_idx")]
    public ulong? InputIdx { get; init; }

    [JsonPropertyName("output_idx")]
    public ulong? OutputIdx { get; init; }

    [JsonPropertyName("output_address")]
    public string? OutputAddress { get; init; }
    
    public ulong? CertificateIdx { get; init; }
}