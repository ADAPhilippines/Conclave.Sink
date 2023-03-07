using TeddySwap.Common.Enums;
using TeddySwap.Common.Models.Response;

namespace TeddySwap.UI.Services;

public class SinkService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ConfigService _configService;

    public SinkService(IHttpClientFactory clientFactory, ConfigService configService)
    {
        _clientFactory = clientFactory;
        _configService = configService;
    }

    public async Task<PaginatedLeaderBoardResponse> GetLeaderboardAsync(LeaderBoardType leaderboardType = LeaderBoardType.Users, int offset = 0, int limit = 10, string? address = null)
    {
        using HttpClient httpClient = _clientFactory.CreateClient();
        string leaderboardTypeString = leaderboardType == LeaderBoardType.Users ? "users" : "badgers";
        if (address is not null && address != string.Empty)
        {
            PaginatedLeaderBoardResponse? response = await httpClient
                .GetFromJsonAsync<PaginatedLeaderBoardResponse>(
                    $"{_configService.SinkApiUrl}/api/v1/leaderboard/{leaderboardTypeString}/address/{address}"
                );
            return response ?? new PaginatedLeaderBoardResponse { Result = new List<LeaderBoardResponse>() { new LeaderBoardResponse() } };
        }
        else
        {
            PaginatedLeaderBoardResponse? response =
                await httpClient
                    .GetFromJsonAsync<PaginatedLeaderBoardResponse>(
                        $"{_configService.SinkApiUrl}/api/v1/leaderboard/{leaderboardTypeString}?offset={offset}&limit={limit}"
                    );
            if (response is null) throw new HttpRequestException("Bad response from GetLeaderboardAsync.");
            return response;
        }
    }

    public async Task<PaginatedLeaderBoardResponse> GetRewardFromAddressesAsync(string[] addresses)
    {
        using HttpClient httpClient = _clientFactory.CreateClient();
        HttpResponseMessage resp = await httpClient
                .PostAsJsonAsync($"{_configService.SinkApiUrl}/api/v1/leaderboard/users/addresses", new { addresses });
        return await resp.Content.ReadFromJsonAsync<PaginatedLeaderBoardResponse>() ?? throw new HttpRequestException("Bad response from GetRewardFromAddressesAsync.");
    }
}