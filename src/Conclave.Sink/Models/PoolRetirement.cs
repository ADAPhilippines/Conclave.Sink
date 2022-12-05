
using System.Text.Json;

namespace Conclave.Sink.Models;

public class PoolRetirement
{
    public string Pool { get; set; } = string.Empty;
    public ulong Epoch { get; set; }
    public Block Block { get; set; } = new();
}