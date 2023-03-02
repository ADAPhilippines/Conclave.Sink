using System.Text.Json.Serialization;
using TeddySwap.Sink.Extensions;

namespace TeddySwap.Sink.Models.Oura;


public enum OuraVariant
{
    Unknown,
    RollBack,
    Block,
    Transaction,
    TxInput,
    TxOutput,
    Order
}

public interface IOuraEvent
{
    public OuraContext? Context { get; init; }
    public string? Fingerprint { get; init; }
    public OuraVariant? Variant { get; init; }
    public ulong? Timestamp { get; init; }
}