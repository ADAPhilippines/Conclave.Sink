
using System.Reflection;
using Conclave.Sink.Reducers;

namespace Conclave.Sink.Extensions;

public static class AddReducersServiceCollectionExtension
{
    public static IServiceCollection AddOuraReducers(this IServiceCollection service)
    {
        Assembly? assembly = Assembly.GetAssembly(typeof(AddReducersServiceCollectionExtension));
        if (assembly is not null)
        {
            foreach (Type reducerType in assembly.GetTypes())
            {
                if (reducerType.GetCustomAttributes(typeof(OuraReducerAttribute), true).Length > 0)
                {
                    service.Add(new ServiceDescriptor(typeof(IOuraReducer), reducerType, ServiceLifetime.Scoped));
                }
            }
        }
        return service;
    }
}