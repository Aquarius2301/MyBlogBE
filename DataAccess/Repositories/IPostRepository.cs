using BusinessObject.Models;

namespace DataAccess.Repositories;

/// <summary>
/// Repository interface for managing <see cref="Post"/> entities.
/// </summary>
public interface IPostRepository : IRepository<Post>
{
    /// <summary>
    /// Gets a post by its unique link.
    /// </summary>
    /// <param name="link">The link (slug) of the post.</param>
    /// <param name="includeDeleted">Whether to include deleted posts.</param>
    /// <returns>
    /// A <see cref="Post"/> entities with the specified link if found; otherwise, null.
    /// </returns>
    public Task<Post?> GetByLinkAsync(string link, bool includeDeleted = false);
}
