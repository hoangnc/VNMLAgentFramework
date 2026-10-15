using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using VnMLAgentFramework.LlamaCppClient.Converter;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    public record Timings
    {
        [JsonPropertyName("cache_n")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? CacheN { get; set; }

        [JsonPropertyName("prompt_n")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? PromptN { get; set; }

        [JsonPropertyName("prompt_ms")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PromptMs { get; set; }

        [JsonPropertyName("prompt_per_token_ms")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PromptPerTokenMs { get; set; }

        [JsonPropertyName("prompt_per_second")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PromptPerSecond { get; set; }

        [JsonPropertyName("predicted_n")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? PredictedN { get; set; }

        [JsonPropertyName("predicted_ms")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PredictedMs { get; set; }

        [JsonPropertyName("predicted_per_token_ms")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PredictedPerTokenMs { get; set; }

        [JsonPropertyName("predicted_per_second")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PredictedPerSecond { get; set; }

        [JsonPropertyName("prompt_progress")]
        public PromptProgress? PromptProgress { get; set; }

        public int SafeCacheN => CacheN ?? 0;
        public int SafePromptN => PromptN ?? 0;
        public double SafePromptMs => PromptMs ?? 0;
        public double SafePromptPerTokenMs => PromptPerTokenMs ?? 0;
        public double SafePromptPerSecond => PromptPerSecond ?? 0;
        public int SafePredictedN => PredictedN ?? 0;
        public double SafePredictedMs => PredictedMs ?? 0;
        public double SafePredictedPerTokenMs => PredictedPerTokenMs ?? 0;
        public double SafePredictedPerSecond => PredictedPerSecond ?? 0;

        public int TotalTokens => SafePromptN + SafePredictedN;
        public double TotalTimeMs => SafePromptMs + SafePredictedMs;
        public bool HasPredictionStarted => PredictedN > 0 || PredictedMs > 0;
        public bool IsPromptPhase => PromptProgress != null;

        public override string ToString()
        {
            if (IsPromptPhase)
            {
                return $"[Prompt] {SafePromptN} tokens in {SafePromptMs:F3}ms " +
                       $"({SafePromptPerSecond:F0} tok/s) | " +
                       $"Progress: {PromptProgress?.Processed}/{PromptProgress?.Total} " +
                       $"({PromptProgress?.ProgressPercentage:F1}%)";
            }

            if (HasPredictionStarted)
            {
                return $"[Prompt] {SafePromptN} tok @ {SafePromptPerSecond:F0} tok/s | " +
                       $"[Gen] {SafePredictedN} tok @ {SafePredictedPerSecond:F1} tok/s " +
                       $"({SafePredictedPerTokenMs:F2}ms/tok)";
            }

            return $"[Prompt] {SafePromptN} tokens in {SafePromptMs:F3}ms ({SafePromptPerSecond:F0} tok/s)";
        }
    }
}
