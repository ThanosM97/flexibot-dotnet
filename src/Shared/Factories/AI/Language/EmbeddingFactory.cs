using Shared.Interfaces.AI.Language;
using Shared.Services.AI.Language;
using Microsoft.Extensions.Configuration;

namespace Shared.Factories.AI.Language
{
    /// <summary>
    /// Factory class to create instances of embedding services based on the specified provider.
    /// </summary>
    public static class EmbeddingFactory
    {
        /// <summary>
        /// Creates an instance of an embedding service based on the specified provider.
        /// </summary>
        /// <param name="provider">The type of embedding service to create. Currently supports "Ollama".</param>
        /// <param name="config">An instance of IConfiguration.</param>
        /// <returns>An instance of IEmbeddingService corresponding to the specified provider.</returns>
        /// <exception cref="NotSupportedException">Thrown when an unsupported embedding provider is specified.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the EMBEDDING_PROVIDER configuration is not set.</exception>
        public static IEmbeddingService GetEmbeddingService(IConfiguration config)
        {
            // Get provider
            string provider = config["EMBEDDING_PROVIDER"] ?? throw new InvalidOperationException("EMBEDDING_PROVIDER has not been set.");

            return provider.ToLower() switch
            {
                "ollama" => new OllamaEmbeddingService(config),
                _ => throw new NotSupportedException($"Unsupported embedding provider: {provider}")
            };
        }
    }
}