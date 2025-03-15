using Microsoft.Extensions.Configuration;

using Shared.Factories.Search;
using Shared.Interfaces.AI.Language;
using Shared.Interfaces.Search;
using Shared.Interfaces.Storage;
using Shared.Services.AI.Language;


namespace Shared.Factories.AI.Language
{
    /// <summary>
    /// Factory class for creating instances of Question-Answering services.
    /// </summary>
    public static class QnAFactory
    {
        /// <summary>
        /// Creates and returns an instance of an IQnAService service based on configuration settings.
        /// </summary>
        /// <param name="config">The configuration object containing settings for the QnA service.</param>
        /// <returns>An instance of a class implementing the IQnAService interface.</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified RAG method is not supported.</exception>
        public static IQnAService GetQnAService(IStorageService storageService, IConfiguration config)
        {
            // Get search service
            IVectorDatabaseService vectorDatabaseService = VectorDatabaseFactory.GetVectorDatabaseService(config);

            // Get embedding service
            IEmbeddingService embdService = EmbeddingFactory.GetEmbeddingService(config);

            // Get RAG method, or set default as simple
            string method = config.GetSection("QNA")["METHOD"] ?? "semantic";

            return method.ToLower() switch
            {
                "semantic" => new SemanticQnAService(vectorDatabaseService, embdService, storageService, config),
                _ => throw new NotSupportedException($"Unsupported QnA method: {method}")
            };
        }
    }
}