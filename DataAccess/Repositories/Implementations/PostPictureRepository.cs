using System;
using BusinessObject;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PostPictureRepository : Repository<PostPicture>, IPostPictureRepository
{
    public PostPictureRepository(MyBlogContext context) : base(context)
    {
    }

    public async Task<List<PostPicture>> GetByPostIdAsync(Guid postId)
    {
        return await _context.PostPictures
                            .Where(pp => pp.PostId == postId)
                            .ToListAsync();

    }
}
