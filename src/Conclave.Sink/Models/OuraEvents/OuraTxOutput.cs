namespace Conclave.Sink.Models.OuraEvents;

public record OuraTxOutput
{
    public string? Address { get; init; }
    public ulong? Amount { get; init; }
    public IEnumerable<OuraAsset>? Assets { get; set; }
}