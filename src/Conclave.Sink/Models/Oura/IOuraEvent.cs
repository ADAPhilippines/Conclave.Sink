using System.Text.Json.Serialization;
using Conclave.Sink.Extensions;

namespace Conclave.Sink.Models.Oura;


public enum OuraVariant
{
    Unknown,
    RollBack,
    Block,
    Transaction,
    TxInput,
    TxOutput,
    StakeDelegation,
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