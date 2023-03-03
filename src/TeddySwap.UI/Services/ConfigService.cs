namespace TeddySwap.UI.Services;

public class ConfigService
{
    private readonly IConfiguration _config;

    public ConfigService(IConfiguration config)
    {
        _config = config;
    }

    public string ExplorerApiUrl => _config["ExplorerApiUrl"] ?? "https://8081-parallel-guidance-uagipf.us1.demeter.run";
    public string SinkApiUrl => _config["SinkApiUrl"] ?? "https://3000-faithful-contract-kybztp.us1.demeter.run";
}