using System;
using BusinessObject;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly MyBlogContext _context;
    public Repository(MyBlogContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        return _context.SaveChangesAsync();
    }

    public Task<List<T>> GetAllAsync()
    {
        return _context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public IQueryable<T> GetQuery()
    {
        return _context.Set<T>().AsQueryable();
    }

    public Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        return _context.SaveChangesAsync();
    }
}
