using System;
using BusinessObject;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class CommentPictureRepository : Repository<CommentPicture>, ICommentPictureRepository
{
    public CommentPictureRepository(MyBlogContext context) : base(context)
    {
    }

    public async Task<List<CommentPicture>> GetByCommentIdAsync(Guid commentId)
    {
        return await _context.CommentPictures
                    .Where(cp => cp.CommentId == commentId)
                    .ToListAsync();
    }
}
