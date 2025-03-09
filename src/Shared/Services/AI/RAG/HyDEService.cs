using Microsoft.Extensions.Configuration;

using Shared.Interfaces.AI.Language;
using Shared.Interfaces.Search;
using Shared.Models;
using Shared.Prompts;


namespace Shared.Services.AI.RAG
{
    /// <summary>
    /// Provides an implementation for the Hypothetical Document Embedding (HyDE) advanced RAG method.
    /// This class uses a vector database service for retrieval, an embedding service for generating embeddings,
    /// and a chat service for generating responses. It is configured using specified settings from an IConfiguration object.
    /// </summary>
    /// <param name="retriever">Service for retrieving relevant data chunks from a vector database.</param>
    /// <param name="embedder">Service for generating embeddings from text.</param>
    /// <param name="generator">Service for generating chat responses.</param>
    /// <param name="config">Configuration object for retrieving RAG settings.</param>
    public class HyDERAG(
        IVectorDatabaseService retriever,
        IEmbeddingService embedder,
        IChatService generator,
        IConfiguration config) : BaseRAG(retriever, embedder, generator, config)
    {
        /// <summary>
        /// Asynchronously generates a vector embedding for a hypothetical document based on the provided chat history.
        /// </summary>
        /// <param name="chatHistory">A list of <see cref="ChatCompletionMessage"/> representing the chat history.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an array of floats,
        /// which is the vector embedding of the hypothetical document generated from the chat history.
        /// </returns>
        protected override async Task<float[]> GetVectorQueryAsync(List<ChatCompletionMessage> chatHistory)
        {
            // Prepend a system instruction for hypothetical document generation
            List<ChatCompletionMessage> chat = [
                new ChatCompletionMessage { Msg = RAGPrompts.HyDEInstruction(), Role = ChatRole.System },
                ..chatHistory
            ];

            // Generate hypothetical document based on query
            string hypotheticalDocument = await _generator.CompleteChatAsync(chat);

            // Generate query embedding using the user prompt (last chat message)
            return await _embedder.GenerateEmbeddingAsync(hypotheticalDocument);
        }

    }
}