using System.Text.Json;

namespace Conclave.Sink.Models;

public record Transaction
{
    public string Hash { get; init; } = string.Empty;
    public ulong Fee { get; init; }
    public IEnumerable<Withdrawal>? Withdrawals { get; init; }
    public Block Block { get; init; } = new();
    public IEnumerable<TxInput> Inputs { get; set; } = new List<TxInput>();
    public IEnumerable<TxOutput> Outputs { get; set; } = new List<TxOutput>();
}