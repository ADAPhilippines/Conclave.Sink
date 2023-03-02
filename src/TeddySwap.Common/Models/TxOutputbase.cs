using System.Text.Json;
namespace TeddySwap.Common.Models;

public partial record TxOutputBase
{
    public string TxHash { get; init; } = string.Empty;
    public Transaction Transaction { get; init; } = new();
    public ulong Index { get; init; }
    public ulong Amount { get; init; }
    public string Address { get; init; } = string.Empty;
    public string? DatumCbor { get; init; }
    public IEnumerable<Asset>? Assets { get; set; }
}