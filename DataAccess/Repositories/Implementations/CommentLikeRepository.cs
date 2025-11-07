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

    public async Task<CommentLike?> GetCommentLikeByPostAndAccountAsync(Guid accountId, Guid commentId)
    {
        return await _context.CommentLikes
                     .FirstOrDefaultAsync(pl => pl.CommentId == commentId && pl.AccountId == accountId);
    }
}
