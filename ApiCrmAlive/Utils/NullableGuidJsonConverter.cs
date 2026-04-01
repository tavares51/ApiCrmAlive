using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiCrmAlive.Utils;

// Allows Guid? to be provided as "" (or whitespace) and treated as null.
// Useful for frontend forms that submit empty strings for optional Guid fields.
public sealed class NullableGuidJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (Guid.TryParse(s, out var g))
                return g;
        }

        throw new JsonException("Invalid GUID value.");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }
}

