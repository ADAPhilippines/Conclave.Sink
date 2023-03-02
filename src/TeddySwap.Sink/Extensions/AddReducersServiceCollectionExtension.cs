
using System.Reflection;
using TeddySwap.Sink.Reducers;

namespace TeddySwap.Sink.Extensions;

public static class AddReducersServiceCollectionExtension
{
    public static IServiceCollection AddOuraReducers(this IServiceCollection service)
    {
        Assembly? assembly = Assembly.GetAssembly(typeof(Program));
        if (assembly is not null)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(OuraReducerAttribute), true).Length > 0)
                {
                    service.Add(new ServiceDescriptor(typeof(IOuraReducer), type, ServiceLifetime.Scoped));
                }
            }
        }
        return service;
    }
}