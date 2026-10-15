using System.Text.Json.Serialization;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public record ErrorInfo
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}
