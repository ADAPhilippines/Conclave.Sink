using System.Reflection;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Reducers;

public class OuraReducerBase : IOuraReducer
{
    public Task HandleReduceAsync(IOuraEvent? _event)
    {
        ArgumentNullException.ThrowIfNull(_event);
        MethodInfo? MI = this.GetType().GetMethod("ReduceAsync");

        if (MI is not null)
        {
            Task? result = MI.Invoke(this, new object[] { _event }) as Task;
            if (result is not null) return result;
        }

        throw new NotImplementedException();
    }

    public Task HandleRollbackAsync(IEnumerable<Block> rollbackBlocks)
    {
        MethodInfo? MI = this.GetType().GetMethod("RollbackAsync");

        if (MI is not null)
        {
            Task? result = MI.Invoke(this, new object[] { rollbackBlocks }) as Task;
            if (result is not null) return result;
        }

        throw new NotImplementedException();
    }

}