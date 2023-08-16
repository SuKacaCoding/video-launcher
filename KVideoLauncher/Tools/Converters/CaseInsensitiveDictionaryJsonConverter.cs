using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KVideoLauncher.Tools.Converters;

public class CaseInsensitiveDictionaryJsonConverter<TValue>
    : JsonConverter<IDictionary<string, TValue>>
{
    public override IDictionary<string, TValue> Read
        (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dict = (IDictionary<string, TValue>?)JsonSerializer
            .Deserialize(ref reader, typeToConvert, options);
        return dict is { }
            ? new Dictionary<string, TValue>(dict, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
    }

    public override void Write(Utf8JsonWriter writer, IDictionary<string, TValue> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize
        (
            writer, value, inputType: value.GetType(),
            options
        );
    }
}