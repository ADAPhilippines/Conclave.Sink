using TeddySwap.Common.Models;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Reducers;

public interface IOuraReducer
{
    Task HandleReduceAsync(IOuraEvent? _event);
    Task HandleRollbackAsync(Block rollbackBlock);
}