using System.Linq.Expressions;

namespace Shared.Interfaces.Database
{
    /// <summary>
    /// Represents a service interface for managing a database.
    /// </summary>
    public interface IDatabaseService<TEntity> where TEntity : class
    {
        /// <summary>
        /// Asynchronously inserts a new entity into the database.
        /// </summary>
        /// <param name="entity">The <see cref="TEntity"/> of the entity to be inserted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertAsync(TEntity entity);

        /// <summary>
        /// Retrieves an entity from the database by its primary key (can be composite).
        /// </summary>
        /// <param name="keys">The unique key(s) of the entity to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified <paramref name="keys"/> is not found.</exception>
        Task<TEntity> GetObjByIdAsync(params object[] keys);

        /// <summary>
        /// Retrieves entities matching the specified filter.
        /// </summary>
        /// <param name="filter">LINQ expression to filter results</param>
        /// <param name="orderBy">LINQ expression to order results</param>
        /// <returns>List of matching entities</returns>
        Task<IEnumerable<TEntity>> GetObjByFilterAsync(
            Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null
        );

        /// <summary>
        /// Retrieves a list of all entities from the database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a list of <see cref="TEntity"/>.
        /// </returns>
        Task<List<TEntity>> ListAsync();

        /// <summary>
        /// Asynchronously updates specific fields of an existing entity in the database.
        /// </summary>
        /// <param name="updates">
        /// A dictionary containing the fields to be updated and their new values.
        /// The key is the name of the field, and the value is the new value for that field.
        /// </param>
        /// <param name="keys">The unique key(s) of the entity to be updated.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified <paramref name="keys"/> is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if the update operation encounters invalid field names or types.</exception>
        Task UpdateAsync(Dictionary<string, object> updates, params object[] keys);

        /// <summary>
        /// Asynchronously deletes an entity from the database.
        /// </summary>
        /// <param name="keys">The unique key(s) of the entity to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified <paramref name="keys"/> is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the entity cannot be deleted due to constraints or dependencies.</exception>
        Task DeleteAsync(params object[] keys);
    }
}