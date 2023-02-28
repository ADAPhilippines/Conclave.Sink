namespace TeddySwap.UI.Models;

public record DummyLItem
{
    public int Rank => new Random().Next(100);
    public string TestnetAddress => "addr_test1qzej75kdsa7k70z2jr5ph4hlzd6e58ndj8d7qwssvkjx94535x6xht8nqt534r96gsc88aausnx8guq6xwxpz85vvkgsgsqqw4";
    public string MainnetAddress => "addr1q8nrqg4s73skqfyyj69mzr7clpe8s7ux9t8z6l55x2f2xuqra34p9pswlrq86nq63hna7p4vkrcrxznqslkta9eqs2nscfavlf";
    public int Points => new Random().Next(1000);
    public int Bonus => new Random().Next(100);
    public int Rewards => new Random().Next(100000);
}