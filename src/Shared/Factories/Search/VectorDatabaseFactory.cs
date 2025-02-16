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
///     <summary>
        /// Gets an instance of a vector database service based on the specified provider name.
        /// </summary>
        /// <param name="provider">The name of the vector database provider.</param>
        /// <param name="config">The configuration object used to initialize the service.</param>
        /// <returns>An instance of <see cref="IVectorDatabaseService"/> corresponding to the provider.</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified provider is not supported.</exception>
        public static IVectorDatabaseService GetVectorSearchService(string provider, IConfiguration config)
        {
            return provider.ToLower() switch
            {
                "qdrant" => new QdrantService(config),
                _ => throw new NotSupportedException($"Unsupported vector database provider: {provider}")
            };
        }
    }
}