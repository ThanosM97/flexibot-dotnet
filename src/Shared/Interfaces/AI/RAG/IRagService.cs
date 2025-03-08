using Shared.Models;


namespace Shared.Interfaces.AI.RAG
{
    /// <summary>
    /// Interface for retrieval-augmented generation operations.
    /// </summary>
    public interface IRetrievalAugmentedGeneration
    {
        /// <summary>
        /// Generates an answer based on a list of chat completion messages.
        /// </summary>
        /// <param name="chat">A list of <see cref="ChatCompletionMessage"/> of the chat.</param>
        /// /// <param name="stream">A boolean value indicating whether to stream the response.</param>
        /// <returns>An asynchronous stream of tuples, where each tuple contains a chunk of the generated response,
        /// a boolean indicating if it is the final chunk, and the extracted confidence score.</returns>
        IAsyncEnumerable<(string, bool, float)> GenerateAnswerAsync(List<ChatCompletionMessage> chat, bool stream);
    }
}