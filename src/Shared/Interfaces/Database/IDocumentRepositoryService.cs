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
        /// Retrieves an entity from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
        Task<TEntity> GetObjByIdAsync(string id);

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
        /// <param name="id">The unique identifier of the entity to be updated.</param>
        /// <param name="updates">
        /// A dictionary containing the fields to be updated and their new values.
        /// The key is the name of the field, and the value is the new value for that field.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified <paramref name="id"/> is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if the update operation encounters invalid field names or types.</exception>
        Task UpdateAsync(string id, Dictionary<string, object> updates);

        /// <summary>
        /// Asynchronously deletes an entity from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified <paramref name="id"/> is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the entity cannot be deleted due to constraints or dependencies.</exception>
        Task DeleteAsync(string id);
    }
}