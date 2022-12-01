
namespace Conclave.Sink.Models;

public class RegistrationByStake
{
    public string StakeHash { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
    public ulong TxIndex { get; set; }
}