using BusinessObject.Models;

namespace DataAccess.Repositories;

/// <summary>
/// Repository interface for managing <see cref="CommentLike"/> entities.
/// </summary>
public interface ICommentLikeRepository : IRepository<CommentLike>
{
    /// <summary>
    /// Gets a <see cref="CommentLike"/> by account ID and comment ID.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <returns>A <see cref="CommentLike"/> if found; otherwise, null.</returns>
    Task<CommentLike?> GetByAccountAndCommentAsync(Guid accountId, Guid commentId);
}
