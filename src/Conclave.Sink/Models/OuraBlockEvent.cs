using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraBlockEvent : OuraEvent
{
    public BlockData? Block { get; init; }
}