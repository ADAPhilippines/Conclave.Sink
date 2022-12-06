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
}