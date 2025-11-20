namespace DataAccess.Repositories;

/// <summary>
/// Generic repository interface for managing entities of type <typeparamref name="T"/>.
/// </summary>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// Adds the <typeparamref name="T"/> entity to the context.
    /// </summary>
    /// <param name="entity">
    /// A <typeparamref name="T"/> object to add.
    /// </param>
    void Add(T entity);

    /// <summary>
    /// Adds a range of <typeparamref name="T"/> entities to the context.
    /// </summary>
    /// <param name="entities">
    /// An enumerable collection of <typeparamref name="T"/> objects to add.
    /// </param>
    void AddRange(IEnumerable<T> entities);

    /// <summary>
    /// Removes the <typeparamref name="T"/> entity from the context.
    /// </summary>
    /// <param name="entity">
    /// A <typeparamref name="T"/> object to remove.
    /// </param>
    void Remove(T entity);

    /// <summary>
    /// Removes a range of <typeparamref name="T"/> entities from the context.
    /// </summary>
    /// <param name="entities">
    /// An enumerable collection of <typeparamref name="T"/> objects to remove.
    /// </param>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Gets the queryable set of entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable"/> of entities of type <typeparamref name="T"/>.</returns>
    IQueryable<T> GetQuery();

    /// <summary>
    /// Gets an entity of type <typeparamref name="T"/> .
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="includeDeleted">Whether to include deleted entities.</param>
    /// <returns>An entity of type <typeparamref name="T"/> if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id, bool includeDeleted = false);

    /// <summary>
    /// Gets all entities of type <typeparamref name="T"/> .
    /// </summary>
    /// <param name="includeDeleted">Whether to include deleted entities.</param>
    /// <returns>An entity of type <typeparamref name="T"/>.</returns>
    Task<ICollection<T>> GetAllAsync(bool includeDeleted = false);
}
