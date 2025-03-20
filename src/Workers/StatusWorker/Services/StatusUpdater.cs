using Shared.Interfaces.Database;
using Shared.Models;


namespace StatusWorker.Services
{
    /// <summary>
    /// Service responsible for updating document status in the database.
    /// </summary>
    public class StatusUpdater(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        /// <summary>
        /// Updates the status of a document.
        /// </summary>
        /// <param name="documentId">The ID of the document to update.</param>
        /// <param name="status">The new status to set for the document.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateDocumentStatusAsync(string documentId, int status)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDatabaseService<DocumentMetadata>>();

            await repo.UpdateAsync(documentId, new Dictionary<string, object>
            {
                { "Status", status }
            });
        }
    }
}