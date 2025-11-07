using System;
using WebApi.Dtos;

namespace WebApi.Services.Interfaces;

public interface IPostService
{
    /// <summary>
    /// Gets a list of posts using cursor-based pagination (infinite scroll style).
    /// </summary>
    /// <param name="cursor">
    /// The timestamp of the last loaded post.
    /// If null → fetch the newest posts.
    /// </param>
    /// <param name="userId">The ID of the viewing user.</param>
    /// <param name="pageSize">The number of posts to return per request.</param>
    /// <returns>A list of posts including like status and images.</returns>
    Task<List<GetPostsResponse>> GetPostsListAsync(DateTime? cursor, Guid userId, int pageSize);

    /// <summary>
    /// Gets a list of posts created by the current user.
    /// </summary>
    /// <param name="cursor">The timestamp of the last loaded post.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="pageSize">The number of posts to return per request.</param>
    /// <returns>A list of posts created by the specified user.</returns>
    Task<List<GetMyPostsResponse>> GetMyPostsListAsync(DateTime? cursor, Guid userId, int pageSize);

    /// <summary>
    /// Gets detailed information of a post by its link (slug).
    /// </summary>
    /// <param name="link">The slug of the post.</param>
    /// <param name="userId">The ID of the user (used to check like status).</param>
    /// <returns>The detailed post information.</returns>
    Task<GetPostDetailResponse?> GetPostByLinkAsync(string link, Guid userId);

    /// <summary>
    /// Likes a post for the given user.
    /// </summary>
    /// <param name="postId">The ID of the post to like.</param>
    /// <param name="userId">The ID of the user performing the like.</param>
    /// <returns>
    /// True if the like was successful,
    /// False if the post does not exist.
    /// </returns>
    Task<bool> LikePostAsync(Guid postId, Guid userId);

    /// <summary>
    /// Cancels (removes) a like from a post.
    /// </summary>
    /// <param name="postId">The ID of the post to unlike.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>
    /// True if the unlike was successful,
    /// False if the post does not exist.
    /// </returns>
    Task<bool> CancelLikePostAsync(Guid postId, Guid userId);
}
