namespace Shared.Models
{
    /// <summary>
    /// Represents a request to initiate a chat with a specified prompt.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Gets or sets the prompt for the chat request.
        /// </summary>
        public string Prompt { get; set; }
    }
}