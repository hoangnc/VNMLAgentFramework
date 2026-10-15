using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public sealed record ChatMessage
    {
        [JsonPropertyName("role")]
        public ChatRole Role { get; set; } = ChatRole.User;

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// Tool calls từ assistant — PHẢI là format API (có id, type, function).
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<ToolCallApi>? ToolCalls { get; set; }

        [JsonPropertyName("tool_call_id")]
        public string? ToolCallId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
