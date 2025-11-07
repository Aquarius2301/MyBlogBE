using System;
using BusinessObject;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostLikeRepository : Repository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(MyBlogContext context) : base(context)
    {
    }

    public async Task<PostLike?> GetPostLikeByPostAndAccountAsync(Guid accountId, Guid postId)
    {
        return await _context.PostLikes
                     .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.AccountId == accountId);
    }
}
