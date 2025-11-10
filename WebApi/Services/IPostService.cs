using BusinessObject.Models;
using WebApi.Dtos;

namespace WebApi.Services;

/// <summary>
/// Post Service Interface
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Gets a post by its unique identifier.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <returns>A <see cref="Post"/> with the specified ID if found; otherwise, null.</returns>
    Task<Post?> GetByIdAsync(Guid commentId);

    /// <summary>
    /// Gets a list of posts using cursor-based pagination (infinite scroll style).
    /// </summary>
    /// <param name="cursor">
    /// The timestamp of the last loaded post.
    /// If null → fetch the newest posts.
    /// </param>
    /// <param name="accountId">The ID of the viewing user.</param>
    /// <param name="pageSize">The number of posts to return per request.</param>
    /// <returns>A list of posts including like status and images.</returns>
    Task<List<GetPostsResponse>> GetPostsListAsync(DateTime? cursor, Guid accountId, int pageSize);

    /// <summary>
    /// Gets a list of posts created by the current user.
    /// </summary>
    /// <param name="cursor">The timestamp of the last loaded post.</param>
    /// <param name="accountId">The ID of the user.</param>
    /// <param name="pageSize">The number of posts to return per request.</param>
    /// <returns>A list of posts created by the specified user.</returns>
    Task<List<GetMyPostsResponse>> GetMyPostsListAsync(
        DateTime? cursor,
        Guid accountId,
        int pageSize
    );

    /// <summary>
    /// Gets detailed information of a post by its link (slug).
    /// </summary>
    /// <param name="link">The slug of the post.</param>
    /// <param name="accountId">The ID of the user (used to check like status).</param>
    /// <returns>The detailed post information.</returns>
    Task<GetPostDetailResponse?> GetPostByLinkAsync(string link, Guid accountId);

    /// <summary>
    /// Likes a post for the given user.
    /// </summary>
    /// <param name="postId">The ID of the post to like.</param>
    /// <param name="accountId">The ID of the user performing the like.</param>
    /// <returns>
    /// True if the like was successful,
    /// False if the post does not exist.
    /// </returns>
    Task LikePostAsync(Guid postId, Guid accountId);

    /// <summary>
    /// Cancels (removes) a like from a post.
    /// </summary>
    /// <param name="postId">The ID of the post to unlike.</param>
    /// <param name="accountId">The ID of the user performing the action.</param>
    /// <returns>
    /// True if the unlike was successful,
    /// False if the post does not exist.
    /// </returns>
    Task CancelLikePostAsync(Guid postId, Guid accountId);

    /// <summary>
    /// Gets a paginated list of top-level comments for a post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="cursor">The timestamp of the last loaded comment (used for pagination).</param>
    /// <param name="accountId">The ID of the viewing user (used to check like status).</param>
    /// <param name="pageSize">The number of comments to load per request.</param>
    /// <returns>
    /// A list of <see cref="GetCommentsResponse"/> objects representing comments for the post.
    /// Returns null if the post does not exist.
    /// </returns>
    Task<List<GetCommentsResponse>> GetPostCommentsList(
        Guid postId,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    );

    /// <summary>
    /// Adds a new post.
    /// </summary>
    /// <param name="request">The post creation request containing post details.</param>
    /// <param name="accountId">The ID of the account creating the post.</param>
    /// <returns>A <see cref="CreatePostResponse" objects representing created post></returns>
    Task<CreatePostResponse> AddPostAsync(CreatePostRequest request, Guid accountId);
}
