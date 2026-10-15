using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public sealed record ChatCompletionRequest
    {
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = true;

        [JsonPropertyName("return_progress")]
        public bool ReturnProgress { get; set; } = true;

        [JsonPropertyName("sse_ping_interval")]
        public int SsePingInterval { get; set; } = 1;

        [JsonPropertyName("reasoning_format")]
        public string ReasoningFormat { get; set; } = "auto";

        [JsonPropertyName("chat_template_kwargs")]
        public ChatTemplateKwargs ChatTemplateKwargs { get; set; } = new();

        [JsonPropertyName("reasoning_control")]
        public bool ReasoningControl { get; set; } = true;

        [JsonPropertyName("backend_sampling")]
        public bool BackendSampling { get; set; } = false;

        [JsonPropertyName("timings_per_token")]
        public bool TimingsPerToken { get; set; } = true;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 50_000;

        [JsonPropertyName("max_completion_tokens")]
        public int MaxCompletionTokens { get; set; } = 50_000;

        /// <summary>
        /// Tool definitions gửi cho LLM.
        /// </summary>
        [JsonPropertyName("tools")]
        public List<ToolDefinition>? Tools { get; set; }

        [JsonPropertyName("tool_choice")]
        public object? ToolChoice { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.3f;

        [JsonPropertyName("grammar")]
        public string? Grammar { get; set; }

        [JsonPropertyName("top_p")]
        public float TopP { get; set; } = 0.8f;
        [JsonPropertyName("top_k")]
        public float TopK { get; set; } = 20f;
        [JsonPropertyName("presence_penalty")]
        public float PresencePenalty { get; set; } = 1.5f;
        [JsonPropertyName("repetition_penalty")]
        public float RepetitionPenalty { get; set; } = 1.0f;
    }
}
