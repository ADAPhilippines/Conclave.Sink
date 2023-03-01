using System.Reflection;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Models.Oura;

namespace TeddySwap.Sink.Reducers;

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

    public Task HandleRollbackAsync(Block rollbackBlock)
    {
        MethodInfo? MI = this.GetType().GetMethod("RollbackAsync");

        if (MI is not null)
        {
            Task? result = MI.Invoke(this, new object[] { rollbackBlock }) as Task;
            if (result is not null) return result;
        }

        throw new NotImplementedException();
    }

}