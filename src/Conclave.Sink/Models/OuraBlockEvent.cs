using System.Text.Json.Serialization;

namespace Conclave.Sink.Models;

public record OuraBlockEvent : OuraEvent
{
    public OuraBlock? Block { get; init; }
}