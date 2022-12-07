namespace Conclave.Common.Models;

public record Transaction
{
    public string Hash { get; init; } = string.Empty;
    public ulong Index { get; set; }
    public ulong Fee { get; init; }
    public Block Block { get; init; } = new();
    public IEnumerable<TxInput> Inputs { get; set; } = new List<TxInput>();
    public IEnumerable<TxOutput> Outputs { get; set; } = new List<TxOutput>();
    public IEnumerable<PoolRegistration> PoolRegistrations { get; set; } = new List<PoolRegistration>();
    public IEnumerable<PoolRetirement> PoolRetirements { get; set; } = new List<PoolRetirement>();
    public IEnumerable<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    public IEnumerable<StakeDelegation> StakeDelegations { get; set; } = new List<StakeDelegation>();
}