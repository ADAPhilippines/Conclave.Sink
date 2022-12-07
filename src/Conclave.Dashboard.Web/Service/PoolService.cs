using Conclave.Dashboard.Web.Models;
using System.Text.Json;
using System.Net.Http.Json;


namespace Conclave.Dashboard.Web.Services;

public class PoolService
{
  public HttpClient _httpClient { get; set; } = default!;

  public PoolService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<List<PoolsModel>> GetPoolsListAsync()
  {
    List<PoolsModel> Pools = await _httpClient.GetFromJsonAsync<List<PoolsModel>>("data/pools.json") ?? new();
    return Pools;
  }

  public async Task<List<PoolsModel>> GetFilteredPoolsListAsync(bool isConclave)
  {
    List<PoolsModel> ListOfPools = await _httpClient.GetFromJsonAsync<List<PoolsModel>>("data/pools.json") ?? new();
    List<PoolsModel> FilteredPools = ListOfPools.FindAll(x => x.IsConclave == isConclave);
    return FilteredPools;
  }

  public async Task<List<PoolsModel>> GetPoolsSearchedList(string filter)
  {
    List<PoolsModel> ListOfPools = await _httpClient.GetFromJsonAsync<List<PoolsModel>>("data/pools.json") ?? new();
    List<PoolsModel> FilteredPools = ListOfPools.FindAll(x => x.Ticker.ToLower().Contains(filter.ToLower()) && x.IsConclave == false);
    return FilteredPools;
  }

  public async Task<List<PoolsModel>> GetPaginatedPools(int page, int count)
  {
    List<PoolsModel> ListOfPools = await _httpClient.GetFromJsonAsync<List<PoolsModel>>("data/pools.json") ?? new();
    int maxIndex = page * count;
    int minIndex = page * count - count + 1;
    List<PoolsModel> PaginatedPools = ListOfPools.FindAll(x => x.Id >= minIndex && x.Id <= maxIndex);

    return PaginatedPools;
  }
}