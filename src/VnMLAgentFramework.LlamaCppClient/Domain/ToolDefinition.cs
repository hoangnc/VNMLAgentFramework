using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    /// <summary>
    /// Tool definition gửi cho LLM trong request.
    /// Format: { "type": "function", "function": { "name": "...", "description": "...", "parameters": {...} } }
    /// </summary>
    public sealed record ToolDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public ToolFunctionDefinition Function { get; set; } = new();
    }
}
