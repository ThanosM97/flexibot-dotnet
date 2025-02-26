using Microsoft.Extensions.Configuration;

using Shared.Interfaces.Search;
using Shared.Services.Search.VectorDatabase;


namespace Shared.Factories.Search
{
    /// <summary>
    /// Factory class for creating instances of vector database services.
    /// </summary>
    public static class VectorDatabaseFactory
    {
        /// <summary>
        /// Gets an instance of a vector database service based on the specified provider name.
        /// </summary>
        /// <param name="config">The configuration object used to initialize the service.</param>
        /// <returns>An instance of <see cref="IVectorDatabaseService"/> corresponding to the provider.</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified provider is not supported.</exception>
        public static IVectorDatabaseService GetVectorDatabaseService(IConfiguration config)
        {
            var searchConfig = config.GetSection("SEARCH");
            // Validate search configuration
            if (
                string.IsNullOrWhiteSpace(searchConfig["PROVIDER"]) ||
                string.IsNullOrWhiteSpace(searchConfig["DOCUMENT_COLLECTION"]) ||
                string.IsNullOrWhiteSpace(searchConfig["VECTOR_SIZE"]) ||
                !int.TryParse(searchConfig["VECTOR_SIZE"], out int _)
            )
            {
                throw new Exception("Invalid search service configuration.");
            }

            // Get provider name from configuration
            string? provider = searchConfig["PROVIDER"];

            return provider?.ToLower() switch
            {
                "qdrant" => new QdrantService(config),
                _ => throw new NotSupportedException($"Unsupported vector database provider: {provider}")
            };
        }
    }
}