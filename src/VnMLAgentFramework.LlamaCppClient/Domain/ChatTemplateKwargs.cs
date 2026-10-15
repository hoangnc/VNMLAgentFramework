using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public sealed record ChatTemplateKwargs
    {
        [JsonPropertyName("enable_thinking")]
        public bool EnableThinking { get; set; } = true;
    }
}
