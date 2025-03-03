using Shared.Models;


namespace Shared.Interfaces.AI.Language
{
    /// <summary>
    /// Interface for chat services that provide chat completion capabilities.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Asynchronously gets a chat completion response based on the provided messages.
        /// </summary>
        /// <param name="messages">A collection of chat completion messages to process.</param>
        /// <param name="stream">A boolean value indicating whether to stream the response.</param>
        /// <returns>An asynchronous stream of tuples, where each tuple contains a chat completion response
        /// and a boolean indicating if it is the final response.</returns>
        IAsyncEnumerable<(string, bool)> CompleteChatAsync(IEnumerable<ChatCompletionMessage> messages, bool stream = true);
    }
}