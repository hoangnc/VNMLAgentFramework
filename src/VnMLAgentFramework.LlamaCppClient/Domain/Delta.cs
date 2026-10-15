using REDox.Json;
using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public sealed record Delta
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("reasoning_content")]
        public string? ReasoningContent { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// Tool calls trong delta có thể là string hoặc object.
        /// Dùng GetToolCallsString() để lấy string.
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public object? ToolCallsRaw { get; set; }

        /// <summary>
        /// Lấy tool_calls dưới dạng string để parse.
        /// </summary>
        public string? GetToolCallsString()
        {
            if (ToolCallsRaw == null) return null;
            if (ToolCallsRaw is string str) return str;
            return JsonSerializer.Serialize(ToolCallsRaw);
        }
    }
}
