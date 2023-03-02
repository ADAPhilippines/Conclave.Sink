namespace TeddySwap.UI.Services;

public class HeartBeatService
{
    private ulong latestBlockNo;
    public ulong LatestBlockNo
    {
        get => latestBlockNo;
        set
        {
            latestBlockNo = value;
            Hearbeat?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? Hearbeat;

}