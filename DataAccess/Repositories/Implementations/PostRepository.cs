using System;
using BusinessObject;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(MyBlogContext context) : base(context)
    {
    }

    public async Task<List<Post>> GetPostsByNameAsync(string name)
    {
        return await _context.Posts
            .Where(p => p.Content.Contains(name) && p.DeletedAt == null)
            .ToListAsync();
    }
}
