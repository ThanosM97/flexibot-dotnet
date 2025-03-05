using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

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
            // List all existing collections
            var collections = await _client.ListCollectionsAsync();

            // Check if the collection already exists
            if (collections.Any(c => c == collectionName)) return;

            // Create a new collection with the specified name and vector size
            await _client.CreateCollectionAsync(
                collectionName,
                new VectorParams
                {
                    Size = (ulong)vectorSize,
                    Distance = Distance.Cosine
                });
        }

        /// <inheritdoc/>
        public async Task UpsertVectorsAsync(
            string collectionName, string fileName, IEnumerable<DocumentChunk> chunks)
        {
            // Convert DocumentChunk objects to PointStruct objects for upsert operation
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

            // Upsert the points into the specified collection in Qdrant
            await _client.UpsertAsync(collectionName, points);
        }

        /// <inheritdoc/>
        public async Task DeletePointsByDocumentIdAsync(string collectionName, string documentId)
        {
            // Delete points matching the filter
            await _client.DeleteAsync(
                collectionName: collectionName,
                filter: MatchKeyword("document_id", documentId)
            );
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchResult>> SearchAsync(
            string collectionName, float[] queryVector, int topK)
        {
            // Perform a search in the specified collection using the query vector
            var searchResults = await _client.SearchAsync(
                collectionName,
                queryVector,
                limit: (ulong)topK
            );

            // Convert the search results to a list of SearchResult objects
            return searchResults.Select(ParseResult);
        }

        /// <summary>
        /// Parses a <see cref="ScoredPoint"/> object into a <see cref="SearchResult"/> object.
        /// </summary>
        /// <param name="point">The <see cref="ScoredPoint"/> object to be parsed.</param>
        /// <returns>A <see cref="SearchResult"/> object containing the parsed data from the <see cref="ScoredPoint"/>.</returns>
        private static SearchResult ParseResult(ScoredPoint point)
        {
            return new SearchResult
            {
                ChunkId=point.Payload["chunk_id"].StringValue,
                DocumentId=point.Payload["document_id"].StringValue,
                Content=point.Payload["content"].StringValue,
                FileName=point.Payload["file_name"].StringValue,
                ConfidenceScore=point.Score
            };
        }
    }
}