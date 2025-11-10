using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(MyBlogContext context)
        : base(context) { }

    public new async Task<ICollection<Comment>> GetAllAsync(bool includeDeleted = false)
    {
        var comments = await _context
            .Comments.Where(p => includeDeleted || p.DeletedAt == null)
            .ToListAsync();

        return comments;
    }

    public new async Task<Comment?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(p =>
            p.Id == id && (includeDeleted || p.DeletedAt == null)
        );

        return comment;
    }
}
