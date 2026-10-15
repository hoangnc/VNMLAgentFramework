using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    // ==================== TOOL DEFINITION (REQUEST → LLM) ====================
    /// <summary>
    /// Represents the definition of a tool function that can be invoked by the LLM.
    /// </summary>
    public sealed record ToolFunctionDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public JsonObject? Parameters { get; set; }
    }
}
