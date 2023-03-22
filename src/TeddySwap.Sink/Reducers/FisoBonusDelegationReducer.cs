// using System.Text.Json;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using TeddySwap.Common.Models;
// using TeddySwap.Sink.Data;
// using TeddySwap.Sink.Models;
// using TeddySwap.Sink.Models.Oura;

// namespace TeddySwap.Sink.Reducers;

// [OuraReducer(OuraVariant.StakeDelegation)]
// public class FisoBonusDelegationReducer : OuraReducerBase
// {
//     private readonly ILogger<FisoBonusDelegationReducer> _logger;
//     private readonly IDbContextFactory<TeddySwapFisoSinkDbContext> _dbContextFactory;

//     public FisoBonusDelegationReducer(
//         ILogger<FisoBonusDelegationReducer> logger,
//         IDbContextFactory<TeddySwapFisoSinkDbContext> dbContextFactory,
//         IOptions<TeddySwapSinkSettings> settings)
//     {
//         _logger = logger;
//         _dbContextFactory = dbContextFactory;
//     }

//     public async Task ReduceAsync(OuraStakeDelegationEvent stakeDelegationEvent)
//     {
//         await Task.CompletedTask;
//     }
//     public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
// }