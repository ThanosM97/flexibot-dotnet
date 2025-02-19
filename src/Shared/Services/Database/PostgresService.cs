using Microsoft.EntityFrameworkCore;

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
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<DocumentMetadata> GetDocumentAsync(string documentId)
        {
            return await _context.Documents.FindAsync(documentId);
        }
    }
}
