namespace Application.Interfaces.IRepositories
{
    /// <summary>
    /// Defines a generic repository for standard CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities from the database asynchronously.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Retrieves an entity by its unique identifier (GUID).
        /// </summary>
        /// <returns>The entity if found, otherwise null.</returns>
        Task<T?> GetByIdAsync(Guid? id);

        /// <summary>
        /// Adds a new entity to the database asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Removes a specific entity from the database.
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Removes an entity by its unique identifier (GUID).
        /// </summary>
        void DeleteById(Guid id);

        // Int-based ID methods

        /// <summary>
        /// Retrieves an entity by its integer identifier.
        /// </summary>
        /// <returns>The entity if found, otherwise null.</returns>
        Task<T?> GetByIdAsync(int? id);

        /// <summary>
        /// Removes an entity by its integer identifier.
        /// </summary>
        void DeleteById(int id);
    }
}