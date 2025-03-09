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
        /// <returns>An asynchronous stream of tuples, where each tuple contains a chat completion response
        /// and a boolean indicating if it is the final response.</returns>
        IAsyncEnumerable<(string, bool)> CompleteChatStreamAsync(IEnumerable<ChatCompletionMessage> messages);

        /// <summary>
        /// Asynchronously gets a chat completion response based on the provided messages.
        /// </summary>
        /// <param name="messages">A collection of chat completion messages to process.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the chat completion response as a string.</returns>
        Task<string> CompleteChatAsync(IEnumerable<ChatCompletionMessage> messages);
    }
}