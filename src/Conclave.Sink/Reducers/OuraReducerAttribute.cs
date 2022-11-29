
using Conclave.Sink.Models;

namespace Conclave.Sink.Reducers;

public class OuraReducerAttribute : System.Attribute
{
    public OuraVariant Variant { get; private set; }

    public OuraReducerAttribute(OuraVariant variant)
    {
        Variant = variant;
    }
}