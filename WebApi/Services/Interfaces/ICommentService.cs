using System;
using WebApi.Dtos;

namespace WebApi.Services.Interfaces;

public interface ICommentService
{
    /// <summary>
    /// Gets a paginated list of top-level comments for a post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="userId">The ID of the viewing user (used to check like status).</param>
    /// <param name="pageSize">The number of comments to load per request.</param>
    /// <returns>A list of comments for the specified post.</returns>
    Task<List<GetCommentsResponse>> GetCommentList(Guid postId, Guid userId, int pageSize);

    /// <summary>
    /// Gets a paginated list of replies for a specific parent comment.
    /// </summary>
    /// <param name="commentId">The ID of the parent comment.</param>
    /// <param name="userId">The ID of the viewing user (used to check like status).</param>
    /// <param name="pageSize">The number of replies to load per request.</param>
    /// <returns>A list of child comments (replies).</returns>
    Task<List<GetChildCommentsResponse>> GetChildCommentList(Guid commentId, Guid userId, int pageSize);

    /// <summary>
    /// Likes a comment for the given user.
    /// </summary>
    /// <param name="commentId">The ID of the comment to like.</param>
    /// <param name="userId">The ID of the user performing the like.</param>
    /// <returns>
    /// null if the comment does not exist,
    /// True if the like action succeeded,
    /// False if the comment was already liked by the user.
    /// </returns>
    Task<bool?> LikeCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// Removes a like from a comment for the given user.
    /// </summary>
    /// <param name="commentId">The ID of the comment to unlike.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>
    /// null if the comment does not exist,
    /// True if the unlike action succeeded,
    /// False if the comment had not been liked by the user before.
    /// </returns>
    Task<bool?> CancelLikeCommentAsync(Guid commentId, Guid userId);
}
