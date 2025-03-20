namespace Shared.Models
{
    /// <summary>
    /// Represents a request to initiate a chat with a specified prompt and optional conversation history.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Gets or sets the session ID for the chat request.
        /// </summary>
        public required string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the prompt for the chat request.
        /// </summary>
        public required string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the number of past messages to include for the response.
        /// Default is 10.
        /// </summary>
        public int PastMessagesIncluded { get; set; } = 10;
    }
}