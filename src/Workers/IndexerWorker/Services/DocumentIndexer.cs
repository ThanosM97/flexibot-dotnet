using Shared.Factories.Search;
using Shared.Interfaces.Search;
using Shared.Models;

namespace IndexerWorker.Services
{
    /// <summary>
    /// Service for indexing documents into a vector database.
    /// </summary>
    public class DocumentIndexer
    {
        private IVectorDatabaseService _client;
        public string? collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentIndexer"/> class with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object containing search service settings.</param>
        /// <exception cref="Exception">Thrown if the search service configuration is invalid.</exception>
        public DocumentIndexer(IConfiguration config)
        {
            var searchConfig = config.GetSection("SEARCH");
            // Validate search configuration
            if (
                string.IsNullOrWhiteSpace(searchConfig["PROVIDER"]) ||
                string.IsNullOrWhiteSpace(searchConfig["DOCUMENT_COLLECTION"]) ||
                string.IsNullOrWhiteSpace(searchConfig["VECTOR_SIZE"]) ||
                int.TryParse(searchConfig["VECTOR_SIZE"], out int vectorSize)
            )
            {
                throw new Exception("Invalid search service configuration.");
            }

            // Set the collection name from the configuration
            collection = searchConfig["DOCUMENT_COLLECTION"];

            // Initialize search client
            _client = VectorDatabaseFactory.GetVectorSearchService(searchConfig["PROVIDER"], config);

            // Create collection if it doesn't exist
            _client.CreateCollectionIfNotExistsAsync(collection, vectorSize);
        }

        /// <summary>
        /// Indexes a list of document chunks into the vector database.
        /// </summary>
        /// <param name="fileName">The name of the file associated with the document chunks.</param>
        /// <param name="chunks">The list of <see cref="DocumentChunk"/> objects to be indexed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task IndexChunksAsync(string fileName, List<DocumentChunk> chunks)
        {
            // Upsert the vectors into the specified collection
             await _client.UpsertVectorsAsync(collection, fileName, chunks);
        }
    }
}
