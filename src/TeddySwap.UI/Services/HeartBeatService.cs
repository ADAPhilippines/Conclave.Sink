namespace TeddySwap.UI.Services;

public class HeartBeatService
{
    private readonly QueryService _queryService;

    public HeartBeatService(QueryService queryService)
    {
        _queryService = queryService;
    }

    private ulong latestBlockNo;
    public ulong LatestBlockNo
    {
        get => latestBlockNo;
        set
        {
            _queryService.Invalidate();
            latestBlockNo = value;
            Hearbeat?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? Hearbeat;

}