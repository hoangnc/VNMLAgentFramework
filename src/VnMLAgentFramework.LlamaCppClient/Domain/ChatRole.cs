using System.ComponentModel;
using System.Text.Json.Serialization;
using VnMLAgentFramework.LlamaCppClient.Converter;

namespace VnMLAgentFramework.LlamaCppClient.Domain
{
    [JsonConverter(typeof(ChatRoleConverter))]
    public readonly struct ChatRole : IEquatable<ChatRole>
    {
        private readonly string _value;

        public ChatRole(string? role)
        {
            _value = role ?? throw new ArgumentNullException(nameof(role));
        }

        [JsonConstructor]
        public ChatRole(object _)
        {
            _value = null!;
        }

        private const string SYSTEM_VALUE = "system";
        private const string ASSISTANT_VALUE = "assistant";
        private const string USER_VALUE = "user";
        private const string TOOL_VALUE = "tool";

        public static ChatRole System { get; } = new(SYSTEM_VALUE);
        public static ChatRole Assistant { get; } = new(ASSISTANT_VALUE);
        public static ChatRole User { get; } = new(USER_VALUE);
        public static ChatRole Tool { get; } = new(TOOL_VALUE);

        public static bool operator ==(ChatRole left, ChatRole right) => left.Equals(right);
        public static bool operator !=(ChatRole left, ChatRole right) => !left.Equals(right);
        public static implicit operator ChatRole(string value) => new(value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj) => obj is ChatRole other && Equals(other);

        public bool Equals(ChatRole other) => string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;

        public override string ToString() => _value;
    }
}
