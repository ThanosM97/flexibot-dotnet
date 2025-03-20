using Microsoft.Extensions.Configuration;

using Shared.Interfaces.AI.Language;
using Shared.Interfaces.Search;
using Shared.Models;


namespace Shared.Services.AI.RAG
{
    /// <summary>
    /// Provides an implementation for Simple Retrieval-Augmented Generation (RAG) using
    /// a vector database service for retrieval, an embedding service for generating
    /// embeddings, and a chat service for generating responses. This class is configured
    /// using specified settings from an IConfiguration object.
    /// </summary>
    /// <param name="retriever">Service for retrieving relevant data chunks from a vector database.</param>
    /// <param name="embedder">Service for generating embeddings from text.</param>
    /// <param name="generator">Service for generating chat responses.</param>
    /// <param name="config">Configuration object for retrieving RAG settings.</param>
    public class SimpleRAG(
        IVectorDatabaseService retriever,
        IEmbeddingService embedder,
        IChatService generator,
        IConfiguration config) : BaseRAG(retriever, embedder, generator, config)
    {

        /// <summary>
        /// Asynchronously generates a vector embedding for the latest chat message in the provided chat history.
        /// </summary>
        /// <param name="chatHistory">A list of chat messages representing the chat history.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an array of floats,
        /// which is the vector embedding of the latest chat message.
        /// </returns>
        protected override async Task<float[]> GetVectorQueryAsync(List<ChatCompletionMessage> chatHistory)
        {
            // Generate query embedding using the user prompt (last chat message)
            return  await _embedder.GenerateEmbeddingAsync(chatHistory.Last().Content);
        }
    }
}