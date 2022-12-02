using System.Text.Json.Serialization;
using Conclave.Sink.Extensions;

namespace Conclave.Sink.Models;


public enum OuraVariant
{
    Unknown,
    RollBack,
    Block,
    TxOutput,
    TxInput,
    PoolRegistration,
    PoolRetirement
}

public interface IOuraEvent
{
    public OuraContext? Context { get; init; }
    public string? Fingerprint { get; init; }
    public OuraVariant? Variant { get; init; }

    public ulong? Timestamp { get; init; }
}