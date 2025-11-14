using System;
using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PictureRepository : Repository<Picture>, IPictureRepository
{
    public PictureRepository(MyBlogContext context)
        : base(context) { }

    public async Task<Picture?> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Pictures.FirstOrDefaultAsync(p => p.AccountId == accountId);
    }

    public async Task<ICollection<Picture>> GetByCommentIdAsync(Guid commentId)
    {
        return await _context.Pictures.Where(p => p.CommentId == commentId).ToListAsync();
    }

    public async Task<ICollection<Picture>> GetByPostIdAsync(Guid postId)
    {
        return await _context.Pictures.Where(p => p.PostId == postId).ToListAsync();
    }
}
