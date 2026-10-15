using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public sealed record Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }
}
