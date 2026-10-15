using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{

    /// <summary>
    /// Represents a message in the chat, which can be from the system, assistant, user, or tool.
    /// </summary>
    public record Message
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// llama.cpp trả tool_calls ở message khi finish_reason = "tool".
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<ToolCall>? ToolCalls { get; set; }
    }
}
