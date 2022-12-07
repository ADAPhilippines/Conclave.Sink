using System.Text.Json;
using System.Text.Json.Serialization;
using Conclave.Common.Models;
using Conclave.Sink.Models.OuraEvents;

namespace Conclave.Sink.Extensions;

public class OuraVariantJsonConverter : JsonConverter<OuraVariant>
{
    public override OuraVariant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        OuraVariant result = OuraVariant.Unknown;
        string? key = reader.GetString();
        if (key is not null)
            Enum.TryParse<OuraVariant>(key, true, out result);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, OuraVariant value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
