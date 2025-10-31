using System;

namespace DataAccess.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetQuery();
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
