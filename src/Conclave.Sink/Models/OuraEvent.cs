using System.Text.Json.Serialization;
using Conclave.Sink.Extensions;

namespace Conclave.Sink.Models;


public enum OuraVariant
{
    Unknown,
    RollBack,
    Block,
    TxOutput,
    TxInput
}

public record OuraEvent
{
    public OuraContext? Context { get; init; }
    public string? Fingerprint { get; init; }

    [JsonConverter(typeof(OuraVariantJsonConverter))]
    public OuraVariant? Variant { get; init; }

    public ulong? Timestamp { get; init; }
}