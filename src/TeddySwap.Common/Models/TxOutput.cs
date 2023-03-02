namespace TeddySwap.Common.Models;

public record TxOutput : TxOutputBase
{
    public IEnumerable<CollateralTxInput> CollateralInputs { get; init; } = new List<CollateralTxInput>();
}