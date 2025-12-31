using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostLikeRepository : Repository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(MyBlogContext context)
        : base(context) { }
}
