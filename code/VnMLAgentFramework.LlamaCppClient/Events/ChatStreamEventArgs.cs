using System;
using System.Collections.Generic;
using System.Text;

namespace VnMLAgentFramework.LlamaCppClient.Events
{
    public class ChatStreamEventArgs : EventArgs
    {
        public ChatChunkResponse? Chunk { get; set; }
        public string? Content { get; set; }
        public string? ThinkingContent { get; set; }
        public ToolCall? ToolCall { get; set; }
        public string? RawJson { get; set; }
        public bool IsComplete { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public int ChunkIndex { get; set; }
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
