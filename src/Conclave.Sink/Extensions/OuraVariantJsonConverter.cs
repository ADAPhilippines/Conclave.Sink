using System.Text.Json;
using System.Text.Json.Serialization;
using Conclave.Sink.Models;
using System.Linq;

namespace Conclave.Sink.Extensions;

public class OuraVariantJsonConverter : JsonConverter<OuraVariant>
{
    private static Dictionary<string, OuraVariant> VariantJsonMap = new Dictionary<string, OuraVariant>()
    {
        ["Block"] = OuraVariant.Block,
        ["TxOutput"] = OuraVariant.TxOutput,
        ["TxInput"] = OuraVariant.TxInput,
    };

    public override OuraVariant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? key = reader.GetString();
        if (key is null || !OuraVariantJsonConverter.VariantJsonMap.ContainsKey(key)) return OuraVariant.Unknown;
        return OuraVariantJsonConverter.VariantJsonMap[key];
    }

    public override void Write(Utf8JsonWriter writer, OuraVariant value, JsonSerializerOptions options)
    {
        if (OuraVariantJsonConverter.VariantJsonMap.ContainsValue(value))
        {
            string? key = OuraVariantJsonConverter.VariantJsonMap.Values
                .Where(v => v == value)
                .Select(v => OuraVariantJsonConverter.VariantJsonMap.Keys
                    .Where(k => OuraVariantJsonConverter.VariantJsonMap[k] == v)
                    .Select(k => k).FirstOrDefault()
                ).FirstOrDefault();

            if (key is null) writer.WriteStringValue(string.Empty);
            else writer.WriteStringValue(key);
        }
    }
}
