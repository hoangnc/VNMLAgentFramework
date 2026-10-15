using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using VnMLAgentFramework.LlamaCppClient.Converter;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    /// <summary>
    /// Represents the progress of a prompt processing operation, including total, cache, processed counts, and time taken.
    /// </summary>
    public record PromptProgress
    {
        [JsonPropertyName("total")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? Total { get; set; }

        [JsonPropertyName("cache")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? Cache { get; set; }

        [JsonPropertyName("processed")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? Processed { get; set; }

        [JsonPropertyName("time_ms")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? TimeMs { get; set; }

        public double ProgressPercentage => (Total ?? 0) > 0 ? (double)(Processed ?? 0) / (Total ?? 1) * 100 : 0;
        public bool IsComplete => (Processed ?? 0) >= (Total ?? int.MaxValue);
    }
}
