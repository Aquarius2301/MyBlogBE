using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class CommentLikeRepository : Repository<CommentLike>, ICommentLikeRepository
{
    public CommentLikeRepository(MyBlogContext context)
        : base(context) { }

    public async Task<CommentLike?> GetByAccountAndCommentAsync(Guid accountId, Guid commentId)
    {
        var commentLike = await _context.CommentLikes.FirstOrDefaultAsync(cl =>
            cl.AccountId == accountId && cl.CommentId == commentId
        );

        return commentLike;
    }
}
