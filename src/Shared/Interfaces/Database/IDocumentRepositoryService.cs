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
    }
}