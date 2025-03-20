using Shared.Interfaces.Database;
using Shared.Interfaces.Search;
using Shared.Models;


namespace DeleterWorker.Services
{
    /// <summary>
    /// Service responsible for deleting documents and their associated vector points from a database.
    /// </summary>
    /// <param name="scopeFactory">Factory for creating service scopes.</param>
    /// <param name="vectorDatabaseService">Service for interacting with the vector database.</param>
    /// <param name="configuration">Configuration settings for the application.</param>
    public class DocumentDeleter(
        IServiceScopeFactory scopeFactory,
        IVectorDatabaseService vectorDatabaseService,
        IConfiguration configuration
    )
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IVectorDatabaseService _vectorDatabaseService = vectorDatabaseService;
        // Get the collection name from the configuration or set a default value
        private readonly string _collectionName = configuration.GetSection("SEARCH")["DOCUMENT_COLLECTION"] ?? "DefaultCollection";

        /// <summary>
        /// Deletes a document from the database using its ID.
        /// </summary>
        /// <param name="documentId">The ID of the document to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteDocumentAsync(string documentId)
        {
            // Create a new service scope to resolve scoped services
            using var scope = _scopeFactory.CreateScope();

            // Get the document repository service from the scope
            var repo = scope.ServiceProvider.GetRequiredService<IDatabaseService<DocumentMetadata>>();

            // Delete the document from the database
            await repo.DeleteAsync(documentId);
        }

        /// <summary>
        /// Deletes the vector points associated with a document from the vector database.
        /// </summary>
        /// <param name="documentId">The ID of the document whose vector points are to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeletePointsAsync(string documentId)
        {
            // Delete the vector points associated with the document
            await _vectorDatabaseService.DeletePointsByDocumentIdAsync(_collectionName, documentId);
        }

    }
}