using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(MyBlogContext context)
        : base(context) { }
}
