using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.OuraEvents;

public record OuraBlockEvent : OuraEvent
{
    public OuraBlock? Block { get; init; }
}