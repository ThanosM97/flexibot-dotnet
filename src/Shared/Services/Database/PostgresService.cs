using Microsoft.EntityFrameworkCore;

using Shared.Events;
using Shared.Interfaces.Database;
using Shared.Models;


namespace Shared.Services.Database
{
    /// <summary>
    /// Represents the database context for document operations, specifically configured for documents.
    /// </summary>
    public class DocumentDbContext(DbContextOptions<DocumentDbContext> options) : DbContext(options)
    {
        public DbSet<DocumentMetadata> Documents { get; set; }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types exposed in <see cref="DbSet{TEntity}"/> properties on the derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentMetadata>();
        }
    }


    /// <summary>
    /// A repository implementation for managing documents in a PostgreSQL database.
    /// </summary>
    /// <param name="context">The <see cref="DocumentDbContext"/> instance to be used for database operations.</param>
    public class PostgresRepository(DocumentDbContext context) : IDocumentRepository
    {
        private readonly DocumentDbContext _context = context;

        /// <inheritdoc/>
        public async Task InsertDocumentAsync(DocumentMetadata document)
        {
            // Add the document to the DbSet
            _context.Documents.Add(document);

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<DocumentMetadata> GetDocumentAsync(string documentId)
        {
            // Find the document by its ID. If not found, throw a KeyNotFoundException
            var document = await _context.Documents.FindAsync(documentId) ?? throw new KeyNotFoundException(
                $"Document with ID {documentId} not found.");

            return document;
        }

        /// <inheritdoc/>
        public async Task<List<DocumentMetadata>> ListDocumentsAsync()
        {
            // Retrieve all documents from the database
            return await _context.Documents.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateDocumentAsync(string documentId, Dictionary<string, object> updates)
        {
            // Find the document by its ID. If not found, throw a KeyNotFoundException
            var document = await _context.Documents.FindAsync(documentId) ?? throw new KeyNotFoundException(
                $"Document with ID {documentId} not found.");

            // Check if the document has been deleted or failed
            if (document.Status == (int)DocumentStatus.Deleted || document.Status == (int)DocumentStatus.Failed)
            {
                throw new KeyNotFoundException($"Document with ID {documentId} not found.");
            }

            // Iterate over each key-value pair in the updates dictionary
            foreach (var (key, value) in updates)
            {
                // Get the property info of the document's metadata by the key
                // If the property does not exist, throw an ArgumentException
                var property = typeof(DocumentMetadata).GetProperty(key) ?? throw new ArgumentException(
                    $"Invalid field name: {key}");

                // Set the value of the property to the new value
                property.SetValue(document, value);
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteDocumentAsync(string documentId)
        {
            // Find the document by its ID. If not found, throw a KeyNotFoundException
            var document = await _context.Documents.FindAsync(documentId) ?? throw new KeyNotFoundException(
                $"Document with ID {documentId} not found.");

            // Remove the document from the DbSet
            _context.Documents.Remove(document);

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
