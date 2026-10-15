using System.Text.Json;
using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Converter
{
    public sealed class NullableDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            if (reader.TokenType == JsonTokenType.Number) return reader.GetDouble();
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrEmpty(str) || str == "null") return null;
                if (double.TryParse(str, out var result)) return result;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteNumberValue(value.Value);
            else writer.WriteNullValue();
        }
    }
}
