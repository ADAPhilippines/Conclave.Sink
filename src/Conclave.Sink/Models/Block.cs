namespace Conclave.Sink.Models;

public class Block
{
    public string BlockHash { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;
    public ulong BlockNumber { get; set; }
    public string VrfKeyhash { get; set; } = string.Empty;
    public ulong Slot { get; set; }
    public ulong Epoch { get; set; }
    public IEnumerable<TxInput> Inputs { get; set; } = new List<TxInput>();
    public IEnumerable<TxOutput> Outputs { get; set; } = new List<TxOutput>();
}