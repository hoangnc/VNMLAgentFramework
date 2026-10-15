using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VnMLAgentFramework.LlamaCppClient.Domain;

namespace VnMLAgentFramework.LlamaCppClient.Converter
{
    /// <summary>
    /// Converts a <see cref="ChatRole"/> to and from JSON.
    /// </summary>
    public sealed class ChatRoleConverter : JsonConverter<ChatRole>
    {
        /// <inheritdoc/>
        public override ChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return new ChatRole(value);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, ChatRole value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
