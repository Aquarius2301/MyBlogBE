using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IPostLikeRepository : IRepository<PostLike>
{
    Task<PostLike?> GetPostLikeByPostAndAccountAsync(Guid accountId, Guid postId);
}
