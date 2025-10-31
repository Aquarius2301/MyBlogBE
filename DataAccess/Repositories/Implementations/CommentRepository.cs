using System;
using BusinessObject;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(MyBlogContext context) : base(context)
    {
    }

    public Task<List<Comment>> GetChildCommentsAsync(Guid commentId)
    {
        return _context.Comments.Where(c => c.ParentCommentId == commentId).ToListAsync();
    }

}
