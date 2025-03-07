using Shared.Factories.AI.Language;
using Shared.Interfaces.AI.Language;
using Shared.Models;


namespace EmbedderWorker.Services
{
    /// <summary>
    /// Service responsible for embedding documents using a specified embedding provider.
    /// </summary>
    /// <param name="config">The configuration object used to retrieve settings.</param>
    /// <exception cref="Exception">Thrown when the embedding provider environment variable is not set.</exception>
    public class DocumentEmbedder(IConfiguration config)
    {
        private readonly IEmbeddingService _client = EmbeddingFactory.GetEmbeddingService(config);

        /// <summary>
        /// Asynchronously generates embedding vectors for a list of document chunks.
        /// </summary>
        /// <param name="chunks">A list of <see cref="DocumentChunk"/>s to embed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task EmbedChunksAsync(List<DocumentChunk> chunks)
        {
            var embeddingTasks = chunks.Select(async chunk =>
            {
                chunk.Embedding = await _client.GenerateEmbeddingAsync(chunk.Content);
            }).ToList();

            await Task.WhenAll(embeddingTasks);
        }
    }
}
