using System.Text.Json;

namespace TeddySwap.Common.Models;

public class MintTransaction
{
    public string PolicyId { get; init; } = string.Empty;
    public string TokenName { get; init; } = string.Empty;
    public string AsciiTokenName { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
}