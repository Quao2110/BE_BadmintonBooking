using Application.Interfaces.IRepositories;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// A generic repository implementation providing standard CRUD operations using Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">The entity type derived from class.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly BadmintonBooking_PRM393Context _context;

        public GenericRepository(BadmintonBooking_PRM393Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously adds a new entity to the database context.
        /// </summary>
        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        /// <summary>
        /// Removes a specific entity from the database context.
        /// </summary>
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        /// <summary>
        /// Finds and removes an entity by its unique identifier (GUID).
        /// </summary>
        public void DeleteById(Guid id)
        {
            var entity = _context.Set<T>().Find(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }

        /// <summary>
        /// Finds and removes an entity by its integer identifier.
        /// </summary>
        public void DeleteById(int id)
        {
            var entity = _context.Set<T>().Find(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }

        /// <summary>
        /// Retrieves all entities of the specified type from the database.
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Retrieves an entity by its unique identifier (GUID).
        /// </summary>
        public async Task<T?> GetByIdAsync(Guid? id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Retrieves an entity by its integer identifier.
        /// </summary>
        public async Task<T?> GetByIdAsync(int? id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Updates an existing entity. 
        /// <para>Note: This method clears the change tracker to avoid conflicts before attaching the modified entity.</para>
        /// </summary>
        public void Update(T entity)
        {
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
        }
    }
}