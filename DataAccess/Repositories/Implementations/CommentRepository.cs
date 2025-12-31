using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(MyBlogContext context)
        : base(context) { }
}
