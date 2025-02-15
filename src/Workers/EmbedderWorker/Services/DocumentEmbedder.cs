using Shared.Factories.AI.Language;
using Shared.Interfaces.AI.Language;
using Shared.Models;

namespace EmbedderWorker.Services
{
    /// <summary>
    /// Service responsible for embedding documents using a specified embedding provider.
    /// </summary>
    public class DocumentEmbedder
    {
        private IEmbeddingService _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEmbedder"/> class.
        /// </summary>
        /// <param name="config">The configuration object used to retrieve settings.</param>
        /// <exception cref="Exception">Thrown when the embedding provider environment variable is not set.</exception>
        public DocumentEmbedder(IConfiguration config)
        {
            string? _provider = config["EMBEDDING_PROVIDER"];
            if (string.IsNullOrWhiteSpace(_provider))
            {
                throw new Exception("Embedding provider env variable has not been set.");
            }

            _client = EmbeddingFactory.GetEmbeddingService(_provider, config);

        }

        /// <summary>
        /// Asynchronously generates embedding vectors for a list of document chunks.
        /// </summary>
        /// <param name="chunks">A list of <see cref="DocumentChunk"/>s to embed.</param>
        /// <returns>A task representing the asynchronous operation, containing a 2D array of embedding vectors.</returns>
        public async Task<float[][]> EmbedChunksAsync(List<DocumentChunk> chunks)
        {
            var chunkTexts = chunks.Select(chunk => chunk.Content).ToList();
            return await _client.GenerateEmbeddingsAsync(chunkTexts);
        }
    }
}
