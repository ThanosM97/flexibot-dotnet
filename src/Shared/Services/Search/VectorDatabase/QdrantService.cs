using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

using Shared.Interfaces.Search;
using Shared.Models;


namespace Shared.Services.Search.VectorDatabase
{
    /// <summary>
    /// Implementation of <see cref="IVectorDatabaseService"/> for interacting with a Qdrant vector database.
    /// </summary>
    public class QdrantService : IVectorDatabaseService
    {
        private readonly QdrantClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantService"/> class with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object containing Qdrant settings.</param>
        /// <exception cref="Exception">Thrown if the Qdrant configuration is invalid.</exception>
        public QdrantService(IConfiguration config)
        {
            var qdrantConfig = config.GetSection("QDRANT");

            // Validate Qdrant configuration
            if (
                string.IsNullOrWhiteSpace(qdrantConfig["HOST"]) ||
                string.IsNullOrWhiteSpace(qdrantConfig["PORT"]) ||
                !int.TryParse(qdrantConfig["PORT"], out int _qdrantPort)
            )
            {
                throw new Exception("Invalid Qdrant Configuration.");
            }

            // Initialize client
            _client = new QdrantClient(qdrantConfig["HOST"], _qdrantPort);
        }

        /// <inheritdoc/>
        public async Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize)
        {
            var collections = await _client.ListCollectionsAsync();
            if (collections.Any(c => c == collectionName)) return;

            await _client.CreateCollectionAsync(
                collectionName,
                new VectorParams
                {
                    Size = (ulong)vectorSize,
                    Distance = Distance.Cosine
                });
        }

        /// <inheritdoc/>
        public async Task UpsertVectorsAsync(string collectionName, string fileName, IEnumerable<DocumentChunk> chunks)
        {
            var points = chunks.Select(chunk => new PointStruct
            {
                Id = new Guid(chunk.Id),
                Vectors = chunk.Embedding,
                Payload =
                {
                    ["document_id"] = chunk.DocumentId,
                    ["content"] = chunk.Content,
                    ["file_name"] = fileName,
                    ["chunk_id"] = chunk.Id
                }
            }).ToList();

            await _client.UpsertAsync(collectionName, points);
        }
    }
}