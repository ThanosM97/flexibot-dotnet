using Shared.Interfaces.Search;
using Shared.Models;

namespace IndexerWorker.Services
{
    /// <summary>
    /// Service for indexing documents into a vector database.
    /// </summary>
    public class DocumentIndexer(IVectorDatabaseService _client, IConfiguration configuration)
    {
        private readonly string _collectionName = configuration.GetSection("SEARCH")["DOCUMENT_COLLECTION"] ?? "DefaultCollection";
        private readonly int _vectorSize = int.Parse(configuration.GetSection("SEARCH")["VECTOR_SIZE"] ?? "384");

        /// <summary>
        /// Indexes a list of document chunks into the vector database.
        /// </summary>
        /// <param name="fileName">The name of the file associated with the document chunks.</param>
        /// <param name="chunks">The list of <see cref="DocumentChunk"/> objects to be indexed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task IndexChunksAsync(string fileName, List<DocumentChunk> chunks)
        {
            // Upsert the vectors into the specified collection
             await _client.UpsertDocumentVectorsAsync(_collectionName, fileName, chunks);
        }

        /// <summary>
        /// Creates a new collection in the vector database if it does not already exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateCollectionIfNotExistsAsync()
        {
            // Create the collection if it does not already exist
            await _client.CreateCollectionIfNotExistsAsync(_collectionName, _vectorSize);
        }
    }
}
