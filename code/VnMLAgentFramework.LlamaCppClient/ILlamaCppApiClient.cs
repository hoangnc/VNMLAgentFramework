using System;
using System.Collections.Generic;
using System.Text;

namespace VnMLAgentFramework.LlamaCppClient
{
    public interface ILlamaCppApiClient : IDisposable
    {
        // Events
        event ChatStreamEventHandler? OnChunkReceived;
        event ChatStreamEventHandler? OnThinkingReceived;
        event ChatStreamEventHandler? OnToolCallReceived;
        event ToolCallStateHandler? OnToolCallStateChanged;
        event ChatStreamEventHandler? OnStreamComplete;
        event ChatStreamEventHandler? OnStreamError;

        // Properties
        bool HasToolSupport { get; }
        IReadOnlyCollection<string> RegisteredTools { get; }

        // Methods
        Task<ServerStartResult?> EnsureServerRunningAsync(string modelPath, int port = 8080, int? maxWaitSeconds = null);
        Task EnsureEmbeddingServerRunningAsync(string modelPath, int port = 11345, int? maxWaitSeconds = null);

        IAsyncEnumerable<ChatStreamItem> ChatStreamAsync(
            string endpoint,
            ChatCompletionRequest request,
            TimeSpan? lineTimeout = null,
            CancellationToken ct = default);

        Task<ChatCompletionResult> ChatAsync(
            string endpoint,
            ChatCompletionRequest request,
            CancellationToken ct = default);

        Task<EmbeddingResponse> EmbeddingAsync(
            string endpoint,
            EmbeddingRequest request,
            CancellationToken ct = default);

        object? GetService(Type serviceType, object? serviceKey = null);
    }
}
