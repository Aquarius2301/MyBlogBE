using BusinessObject.Models;
using WebApi.Dtos;

namespace WebApi.Services;

/// <summary>
/// Comment Service Interface
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Gets a comment by its unique identifier.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <returns> A <see cref="Comment"/> with the specified ID if found; otherwise, null.</returns>
    Task<Comment?> GetByIdAsync(Guid commentId);

    /// <summary>
    /// Gets a paginated list of replies for a specific parent comment.
    /// </summary>
    /// <param name="commentId">The ID of the parent comment.</param>
    /// <param name="cursor">Timestamp of the last loaded child comment (used for pagination).</param>
    /// <param name="accountId">The ID of the viewing user (used to check like status).</param>
    /// <param name="pageSize">The number of replies to load per request.</param>
    /// <returns>
    /// A list of <see cref="GetChildCommentsResponse"/> objects representing child comments.
    /// </returns>
    Task<List<GetChildCommentsResponse>> GetChildCommentList(
        Guid commentId,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    );

    /// <summary>
    /// Likes a comment for the given user.
    /// </summary>
    /// <param name="commentId">The ID of the comment to like.</param>
    /// <param name="accountId">The ID of the user performing the like.</param>
    /// <returns>
    /// True if the like action succeeded,
    /// False if the comment does not exist,
    /// </returns>
    Task<bool> LikeCommentAsync(Guid commentId, Guid accountId);

    /// <summary>
    /// Removes a like from a comment for the given user.
    /// </summary>
    /// <param name="commentId">The ID of the comment to unlike.</param>
    /// <param name="accountId">The ID of the user performing the action.</param>
    /// <returns>
    /// True if the unlike action succeeded,
    /// False if the comment does not exist,
    /// </returns>
    Task<bool> CancelLikeCommentAsync(Guid commentId, Guid accountId);

    Task<CreateCommentResponse> AddCommentAsync(Guid accountId, CreateCommentRequest request);

    Task<UpdateCommentResponse?> UpdateCommentAsync(
        Guid commentId,
        UpdateCommentRequest request,
        Guid accountId
    );

    Task<bool> DeleteCommentAsync(Guid commentId, Guid accountId);
}
