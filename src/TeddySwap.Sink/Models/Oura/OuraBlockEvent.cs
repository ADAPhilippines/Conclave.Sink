using System.Text.Json.Serialization;

namespace TeddySwap.Sink.Models.Oura;

public record OuraBlockEvent : OuraEvent
{
    public OuraBlock? Block { get; init; }
}