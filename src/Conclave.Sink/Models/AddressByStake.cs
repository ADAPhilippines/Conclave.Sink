
namespace Conclave.Sink.Models;

public class AddressByStake
{
    public string StakeAddress { get; set; } = string.Empty;
    public List<string> PaymentAddresses { get; set; } = new List<string>();
}