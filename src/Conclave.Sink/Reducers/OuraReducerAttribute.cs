
using Conclave.Common.Models;
using Conclave.Sink.Models.OuraEvents;

namespace Conclave.Sink.Reducers;

public class OuraReducerAttribute : System.Attribute
{
    public ICollection<OuraVariant> Variants { get; private set; }

    public OuraReducerAttribute(params OuraVariant[] variants)
    {
        Variants = variants.ToList();
    }
}