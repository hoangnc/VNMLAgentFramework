using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public record ChatChunkResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string? SystemFingerprint { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("timings")]
        public Timings? Timings { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }

        [JsonPropertyName("error")]
        public ErrorInfo? Error { get; set; }

        public bool IsDone() => Choices != null && Choices.Any(c =>
            c.FinishReason == "stop" || c.FinishReason == "eos" || c.FinishReason == "tool");

        public bool HasThinking() => Choices?.Any(c =>
            !string.IsNullOrEmpty(c.Delta?.ReasoningContent)) ?? false;

        public bool HasToolCalls() => Choices?.Any(c =>
            c.Message?.ToolCalls != null && c.Message.ToolCalls.Count > 0) ?? false;

        public string? GetThinkingContent() => Choices?.FirstOrDefault()?.Delta?.ReasoningContent;

        public string? GetContent() => Choices?.FirstOrDefault()?.Delta?.Content;

        public List<ToolCall>? GetToolCalls() => Choices?.FirstOrDefault()?.Message?.ToolCalls;
    }
}
