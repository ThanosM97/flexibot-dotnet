using Shared.Models;

namespace Shared.Interfaces.Search
{
    /// <summary>
    /// Interface for vector database services that handle operations related to vector data storage and management.
    /// </summary>
    public interface IVectorDatabaseService
    {
        /// <summary>
        /// Creates a new collection in the vector database if it does not already exist.
        /// </summary>
        /// <param name="collectionName">The name of the collection to create.</param>
        /// <param name="vectorSize">The size of the vectors to be stored in the collection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize);

        /// <summary>
        /// Inserts or updates vectors in the specified collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to upsert vectors.</param>
        /// <param name="fileName">The name of the file associated with the vectors.</param>
        /// <param name="chunks">A collection of <see cref="DocumentChunk"/> objects representing the data to be upserted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertVectorsAsync(string collectionName, string fileName, IEnumerable<DocumentChunk> chunks);
    }
}