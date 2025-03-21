using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

using Shared.Events;
using Shared.Interfaces.Database;
using Shared.Models;


namespace Shared.Services.Database
{
    /// <summary>
    /// Represents the database context for entity operations, specifically configured for entities.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<DocumentMetadata> Entities { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types exposed in <see cref="DbSet{TEntity}"/> properties on the derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentMetadata>();

            modelBuilder.Entity<ChatLog>(entity =>
            {
                // Configure composite key
                entity.HasKey(e => new { e.SessionId, e.MessageId });

                // Ignore auto-generated columns
                entity.Ignore(e => e.RequestYear);
                entity.Ignore(e => e.RequestMonth);
            });
        }
    }


    /// <summary>
    /// A repository implementation for managing entities in a PostgreSQL database.
    /// </summary>
    /// <param name="context">The <see cref="AppDbContext"/> instance to be used for database operations.</param>
    public class PostgresRepository<TEntity>(AppDbContext context) : IDatabaseService<TEntity> where TEntity: class
    {
        private readonly AppDbContext _context = context;

        /// <inheritdoc/>
        public async Task InsertAsync(TEntity entity)
        {
            // Add the entity to the DbSet
            await _context.Set<TEntity>().AddAsync(entity);

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<TEntity> GetObjByIdAsync(params object[] keys)
        {
            // Validate input
            if (keys == null || keys.Length == 0)
                throw new ArgumentException("At least one key must be provided");

            // Find the entity by its key(s). If not found, throw a KeyNotFoundException
            return await _context.Set<TEntity>().FindAsync(keys) ?? throw new KeyNotFoundException(
                $"{typeof(TEntity).Name} with keys ({string.Join(", ", keys)}) not found.");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> GetObjByFilterAsync(
            Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            // Apply the filter expression
            query = query.Where(filter);

            // Apply the order by expression if provided
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Return entities from the database that match the filter
            return await query.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<TEntity>> ListAsync()
        {
            // Retrieve all entities from the database
            return await _context.Set<TEntity>().ToListAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Dictionary<string, object> updates, params object[] keys)
        {
            // Find the entity by its key(s). If not found, throw a KeyNotFoundException
            var entity = await GetObjByIdAsync(keys);
            var properties = typeof(TEntity).GetProperties();

            // Check if the entity is a DocumentMetadata
            if (entity is DocumentMetadata document)
            {
                // Check if the entity has been deleted or failed
                if (document.Status == (int)DocumentStatus.Deleted || document.Status == (int)DocumentStatus.Failed)
                {
                    throw new KeyNotFoundException($"Document with ID ({string.Join(", ", keys)}) not found.");
                }
            }

            // Iterate over each key-value pair in the updates dictionary
            foreach (var (key, value) in updates)
            {
                // Get the property info of the entity's metadata by the key
                // If the property does not exist, throw an ArgumentException
                var property = properties.FirstOrDefault(
                    p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"Property {key} not found in entity.");

                // Set the value of the property to the new value
                property.SetValue(entity, value);
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(params object[] keys)
        {
            // Find the entity by its key(s). If not found, throw a KeyNotFoundException
            var entity = await GetObjByIdAsync(keys);

            // Remove the entity from the DbSet
            _context.Set<TEntity>().Remove(entity);

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
