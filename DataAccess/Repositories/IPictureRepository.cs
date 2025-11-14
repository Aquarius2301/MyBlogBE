using System;
using BusinessObject.Models;

namespace DataAccess.Repositories;

/// <summary>
/// Repository interface for managing <see cref="Picture"/> entities.
/// </summary>
public interface IPictureRepository : IRepository<Picture>
{
    /// <summary>
    /// Gets a <see cref="Picture"/> by account ID.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <returns>A <see cref="Picture"/> if found; otherwise, null.</returns>
    Task<Picture?> GetByAccountIdAsync(Guid accountId);

    /// <summary>
    /// Gets all <see cref="Picture"/> by post ID.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <returns>A collection of <see cref="Picture"/> entities associated with the post.</returns>
    Task<ICollection<Picture>> GetByPostIdAsync(Guid postId);

    /// <summary>
    /// Gets all <see cref="Picture"/>by comment ID.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <returns>A collection of <see cref="Picture"/> entities associated with the comment.</returns>
    Task<ICollection<Picture>> GetByCommentIdAsync(Guid commentId);
}
