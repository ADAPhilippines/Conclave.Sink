using System.Text.Json;
using TeddySwap.Common.Models.Response;

namespace TeddySwap.UI.Models;

public class LeaderBoardItem : LeaderBoardResponse
{
    public bool IsExpanded { get; set; }

    public static LeaderBoardItem? FromResponse(LeaderBoardResponse response)
    {
        return JsonSerializer.Deserialize<LeaderBoardItem>(JsonSerializer.Serialize(response));
    }
}