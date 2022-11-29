using Conclave.Sink.Models;

namespace Conclave.Sink.Reducer;

public interface IOuraReducer
{
    Task ReduceAsync(OuraEvent _event);
    Task HandleRollbackAsync(OuraEvent _event);
}