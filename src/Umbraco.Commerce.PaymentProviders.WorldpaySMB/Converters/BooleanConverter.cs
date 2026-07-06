using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Converters;

public class BooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
        {
            return true;
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return false;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return false;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
