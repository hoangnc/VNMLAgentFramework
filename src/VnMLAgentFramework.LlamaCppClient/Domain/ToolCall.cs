using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    /// <summary>
    /// Tool call từ LLM response.
    /// Format: { "name": "python", "arguments": "{\"code\":\"print(1)\"}" }
    /// </summary>
    public class ToolCall
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = string.Empty;
    }
}
