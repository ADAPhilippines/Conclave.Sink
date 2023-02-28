
using TeddySwap.Common.Models;
using TeddySwap.Sink.Models.Oura;

namespace Conclave.Sink.Reducers;

public class OuraReducerAttribute : System.Attribute
{
    public ICollection<OuraVariant> Variants { get; private set; }

    public OuraReducerAttribute(params OuraVariant[] variants)
    {
        Variants = variants.ToList();
    }
}