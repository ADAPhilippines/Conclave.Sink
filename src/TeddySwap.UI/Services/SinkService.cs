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
        HttpClient httpClient = _clientFactory.CreateClient();
        string leaderboardTypeString = leaderboardType == LeaderBoardType.Users ? "users" : "badgers";
        if (address is not null && address != string.Empty)
        {
            LeaderBoardResponse? response = await httpClient
                .GetFromJsonAsync<LeaderBoardResponse>(
                    $"{_configService.SinkApiUrl}/api/v1/leaderboard/{leaderboardTypeString}/address/{address}"
                );
            if (response is null) throw new HttpRequestException("Bad response from GetLeaderboardAsync.");
            return new PaginatedLeaderBoardResponse
            {
                TotalAmount = response.Total,
                TotalCount = 1,
                Result = new List<LeaderBoardResponse>() { response }
            };
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
}