namespace Conclave.Common.Models;

public record Transaction
{
    public string Hash { get; init; } = string.Empty;
    public ulong Index { get; init; }
    public ulong Fee { get; init; }
    public Block Block { get; init; } = new();
    public IEnumerable<TxInput> Inputs { get; init; } = new List<TxInput>();
    public IEnumerable<CollateralTxInput> CollateralInputs { get; init; } = new List<CollateralTxInput>();
    public IEnumerable<TxOutput> Outputs { get; init; } = new List<TxOutput>();
    public CollateralTxOutput? CollateralOutput { get; init; }
    public IEnumerable<PoolRegistration> PoolRegistrations { get; init; } = new List<PoolRegistration>();
    public IEnumerable<PoolRetirement> PoolRetirements { get; init; } = new List<PoolRetirement>();
    public IEnumerable<Withdrawal> Withdrawals { get; init; } = new List<Withdrawal>();
    public IEnumerable<StakeDelegation> StakeDelegations { get; init; } = new List<StakeDelegation>();
}