using Conclave.Common.Models;
using Conclave.Sink.Models.Oura;

namespace Conclave.Sink.Reducers;

public interface IOuraReducer
{
    Task HandleReduceAsync(IOuraEvent? _event);
    Task HandleRollbackAsync(Block rollbackBlock);
}