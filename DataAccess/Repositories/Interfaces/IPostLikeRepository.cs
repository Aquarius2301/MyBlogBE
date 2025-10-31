using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IPostLikeRepository : IRepository<PostLike>
{
    Task<List<PostLike>> GetPostLikeAsync(Guid postId);
}
