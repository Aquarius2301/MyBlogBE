using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostLikeRepository : Repository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(MyBlogContext context)
        : base(context) { }

    public async Task<PostLike?> GetByAccountAndPostAsync(Guid accountId, Guid postId)
    {
        var postLike = await _context.PostLikes.FirstOrDefaultAsync(pl =>
            pl.AccountId == accountId && pl.PostId == postId
        );

        return postLike;
    }
}
