using BusinessObject.Models;

namespace DataAccess.Repositories;

/// <summary>
/// Repository interface for managing <see cref="PostLike"/> entities.
/// </summary>
public interface IPostLikeRepository : IRepository<PostLike>
{
    /// <summary>
    /// Gets a <see cref="PostLike"/> by account ID and comment ID.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <returns>A <see cref="PostLike"/> if found; otherwise, null.</returns>
    Task<PostLike?> GetByAccountAndPostAsync(Guid accountId, Guid postId);
}
