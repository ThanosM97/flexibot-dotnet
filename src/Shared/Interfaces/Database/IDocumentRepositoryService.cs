using Shared.Models;


namespace Shared.Interfaces.Database
{
    /// <summary>
    /// Represents a repository interface for managing document metadata in a database.
    /// </summary>
    public interface IDocumentRepository
    {
        /// <summary>
        /// Asynchronously inserts a new document into the database.
        /// </summary>
        /// <param name="document">The <see cref="DocumentMetadata"/> of the document to be inserted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertDocumentAsync(DocumentMetadata document);

        /// <summary>
        /// Asynchronously retrieves a document's metadata from the database using the specified document ID.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the document metadata.</returns>
        Task<DocumentMetadata> GetDocumentAsync(string documentId);

        /// <summary>
        /// Retrieves a list of all document metadata from the database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a list of <see cref="DocumentMetadata"/>.
        /// </returns>
        Task<List<DocumentMetadata>> ListDocumentsAsync();

        /// <summary>
        /// Asynchronously updates specific fields of an existing document in the database.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to be updated.</param>
        /// <param name="updates">
        /// A dictionary containing the fields to be updated and their new values.
        /// The key is the name of the field, and the value is the new value for that field.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the document with the specified <paramref name="documentId"/> is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if the update operation encounters invalid field names or types.</exception>
        Task UpdateDocumentAsync(string documentId, Dictionary<string, object> updates);

        /// <summary>
        /// Asynchronously deletes a document from the database.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the document with the specified <paramref name="documentId"/> is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the document cannot be deleted due to constraints or dependencies.</exception>
        Task DeleteDocumentAsync(string documentId);
    }
}