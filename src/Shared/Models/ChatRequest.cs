namespace Shared.Models
{
    /// <summary>
    /// Represents a request to initiate a chat with a specified prompt and optional conversation history.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Gets or sets the prompt for the chat request.
        /// </summary>
        public required string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the conversation history.
        /// This property is optional; if provided, it represents the previous chat messages.
        /// </summary>
        public List<ChatCompletionMessage>? History { get; set; }
    }
}