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
        Task UpsertDocumentVectorsAsync(string collectionName, string fileName, IEnumerable<DocumentChunk> chunks);

        /// <summary>
        /// Inserts or updates vectors in the specified collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to upsert vectors.</param>
        /// <param name="records">A collection of <see cref="QnARecord"/> objects representing the data to be upserted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertQnAVectorsAsync(string collectionName, IEnumerable<QnARecord> records);

        /// <summary>
        /// Deletes points associated with the specified document ID.
        /// </summary>
        /// <param name="collectionName">The name of the collection from which to delete points.</param>
        /// <param name="documentId">The id of the document for which to delete points.</param>
        /// <returns></returns>
        Task DeletePointsByDocumentIdAsync(string collectionName, string documentId);

        /// <summary>
        /// Deletes all points in collection <paramref name="collectionName"/>.
        /// </summary>
        /// <param name="collectionName">The name of the collection from which to delete points.</param>
        /// <returns></returns>
        Task DeleteAllPointsAsync(string collectionName);

        /// <summary>
        /// Searches for the nearest vectors to the specified query vector within the collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to search.</param>
        /// <param name="queryVector">The vector to use as the query for the search.</param>
        /// <param name="topK">The number of top results to return.</param>
        /// <returns>A task representing the asynchronous operation, with a result of an enumerable
        /// collection of <see cref="SearchResult"/> objects representing the nearest vectors found.</returns>
        Task<IEnumerable<SearchResult>> SearchAsync(string collectionName, float[] queryVector, int topK);
    }
}