using System;
using BusinessObject;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class CommentLikeRepository : Repository<CommentLike>, ICommentLikeRepository
{
    public CommentLikeRepository(MyBlogContext context) : base(context)
    {
    }

    public Task<CommentLike?> GetCommentLikeAsync(Guid commentId)
    {
        return _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId);
    }
}
