namespace Conclave.Common.Models.Entities;

public record TxOutput : TxOutputBase
{
    public IEnumerable<CollateralTxInput> CollateralInputs { get; init; } = new List<CollateralTxInput>();
}