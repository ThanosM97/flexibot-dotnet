namespace Shared.Models
{
    /// <summary>
    /// Represents a message used in chat completion interactions, including the role and message content.
    /// </summary>
    public class ChatCompletionMessage
    {
        /// <summary>
        /// Gets or sets the role of the message sender, such as "User", "Assistant", or "System".
        /// </summary>
        public required string Role { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public required string Msg { get; set; }
    }

    /// <summary>
    /// Defines the roles a chat completion message can have.
    /// </summary>
    public static class ChatRole
    {
        public const string System = "System";
        public const string User = "User";
        public const string Assistant = "Assistant";
    }
}