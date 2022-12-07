using System.Text.Json.Serialization;

namespace Conclave.Sink.Models.Oura;

public record OuraBlockEvent : OuraEvent
{
    public OuraBlock? Block { get; init; }
}