using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ICommentLikeRepository : IRepository<CommentLike>
{
    Task<CommentLike?> GetCommentLikeByPostAndAccountAsync(Guid accountId, Guid commentId);
}
