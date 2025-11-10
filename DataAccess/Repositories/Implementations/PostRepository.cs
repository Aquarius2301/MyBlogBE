using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(MyBlogContext context)
        : base(context) { }

    public new async Task<ICollection<Post>> GetAllAsync(bool includeDeleted = false)
    {
        var posts = await _context
            .Posts.Where(p => includeDeleted || p.DeletedAt == null)
            .ToListAsync();

        return posts;
    }

    public new async Task<Post?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p =>
            p.Id == id && (includeDeleted || p.DeletedAt == null)
        );

        return post;
    }
}
