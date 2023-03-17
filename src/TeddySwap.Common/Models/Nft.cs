using System.Text.Json;

namespace TeddySwap.Common.Models;

public class Nft
{
    public string PolicyId { get; init; } = string.Empty;
    public string TokenName { get; init; } = string.Empty;
    public string AsciiTokenName { get; init; } = string.Empty;
    public JsonElement? Metadata { get; init; }
}