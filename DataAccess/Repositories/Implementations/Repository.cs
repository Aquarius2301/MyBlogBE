using BusinessObject;

namespace DataAccess.Repositories.Implementations;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly MyBlogContext _context;

    public Repository(MyBlogContext context)
    {
        _context = context;
    }

    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public async Task<ICollection<T>> GetAllAsync(bool includeDeleted = false)
    {
        throw new NotImplementedException();
    }

    public async Task<T?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        throw new NotImplementedException();
    }

    public IQueryable<T> GetQuery()
    {
        return _context.Set<T>().AsQueryable();
    }

    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }
}
