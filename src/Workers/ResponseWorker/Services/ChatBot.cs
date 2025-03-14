using Shared.Factories.AI.RAG;
using Shared.Interfaces.AI.RAG;
using Shared.Models;


namespace ResponseWorker.Services
{
    /// <summary>
    /// Represents a chatbot service that utilizes a Retrieval-Augmented Generation (RAG) client
    /// to process and generate chat responses based on user input and chat history.
    /// </summary>
    public class ChatBot(IConfiguration config)
    {
        // Create RAG service
        private readonly IRetrievalAugmentedGeneration _client = RAGFactory.GetRAGService(config);

        /// <summary>
        /// Asynchronously processes a chat history and user prompt to generate response chunks.
        /// </summary>
        /// <param name="history">The existing chat history as a list of messages.</param>
        /// <param name="prompt">The current user prompt to be processed.</param>
        /// <param name="stream">Indicates whether the response should be streamed.</param>
        /// <returns>An asynchronous stream of tuples containing the response chunk, a completion flag, and a confidence score.</returns>
        public async IAsyncEnumerable<ChatBotResult> CompleteChunkAsync(
            List<ChatCompletionMessage> history, string prompt, bool stream=true)
        {
            // Initialize the chat history with existing messages and add the user's current prompt
            List<ChatCompletionMessage> chat =
            [
                .. history ?? Enumerable.Empty<ChatCompletionMessage>(),
                new ChatCompletionMessage { Msg = prompt, Role = ChatRole.User },
            ];

            // Yield chunks of the response
            await foreach(RAGResult ragResult in _client.GenerateAnswerAsync(chat, stream))
            {
                yield return new ChatBotResult(
                    ragResult.IsFinalChunk, ragResult.Answer, ragResult.Confidence, "RAG");
            }
        }
    }
}
