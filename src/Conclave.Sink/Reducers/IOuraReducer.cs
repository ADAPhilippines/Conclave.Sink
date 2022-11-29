using Conclave.Sink.Models;

namespace Conclave.Sink.Reducers;

public interface IOuraReducer
{
    Task HandleReduceAsync(IOuraEvent? _event);
    Task HandleRollbackAsync(IOuraEvent? _event);
}