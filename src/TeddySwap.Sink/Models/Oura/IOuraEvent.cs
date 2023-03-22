using System.Text.Json.Serialization;
using TeddySwap.Sink.Extensions;

namespace TeddySwap.Sink.Models.Oura;


public enum OuraVariant
{
    Unknown = 0,
    RollBack = 1,
    Block = 2,
    Transaction = 3,
    TxInput = 4,
    TxOutput = 5,
    Asset = 6,
    StakeDelegation = 7
}

public interface IOuraEvent
{
    public OuraContext? Context { get; set; }
    public string? Fingerprint { get; init; }
    public OuraVariant? Variant { get; init; }
    public ulong? Timestamp { get; init; }
}