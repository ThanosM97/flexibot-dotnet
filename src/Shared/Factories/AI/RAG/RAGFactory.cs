using Microsoft.Extensions.Configuration;

using Shared.Factories.AI.Language;
using Shared.Factories.Search;
using Shared.Interfaces.AI.Language;
using Shared.Interfaces.AI.RAG;
using Shared.Interfaces.Search;
using Shared.Services.AI.RAG;


namespace Shared.Factories.AI.RAG
{
    /// <summary>
    /// Factory class for creating instances of Retrieval-Augmented Generation (RAG) services.
    /// </summary>
    public static class RAGFactory
    {
        /// <summary>
        /// Creates and returns an instance of an IRetrievalAugmentedGeneration service based on configuration settings.
        /// </summary>
        /// <param name="config">The configuration object containing settings for the RAG service.</param>
        /// <returns>An instance of a class implementing the IRetrievalAugmentedGeneration interface.</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified RAG method is not supported.</exception>
        public static IRetrievalAugmentedGeneration GetRAGService(IConfiguration config)
        {
            // Get embedding service
            IEmbeddingService embdService = EmbeddingFactory.GetEmbeddingService(config);

            // Get chat service
            IChatService chatService = ChatFactory.GetChatService(config);

            // Get search service
            IVectorDatabaseService searchService = VectorDatabaseFactory.GetVectorDatabaseService(config);

            // Get RAG method, or set default as simple
            string method = config.GetSection("RAG")["METHOD"] ?? "simple";

            return method.ToLower() switch
            {
                "simple" => new SimpleRAG(searchService, embdService, chatService, config),
                _ => throw new NotSupportedException($"Unsupported RAG method: {method}")
            };
        }
    }
}