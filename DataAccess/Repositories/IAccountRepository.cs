using BusinessObject.Enums;
using BusinessObject.Models;

namespace DataAccess.Repositories;

/// <summary>
/// Repository interface for managing <see cref="Account"/> entities.
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// Gets all accounts with optional filters for inactive and deleted accounts.
    /// </summary>
    /// <param name="includeDeleted">Whether to include deleted accounts.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// A collection of <see cref="Account"/> entities.
    /// </returns>
    Task<ICollection<Account>> GetAllAsync(
        bool includeDeleted = false,
        bool includeInactive = false
    );

    /// <summary>
    /// Gets an account by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the account.</param>
    /// <param name="includeDeleted">Whether to include deleted accounts.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// An <see cref="Account"/> with the specified ID if found; otherwise, null.
    /// </returns>
    Task<Account?> GetByIdAsync(Guid id, bool includeDeleted = false, bool includeInactive = false);

    /// <summary>
    /// Gets an account by its username.
    /// </summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="includeDeleted">Whether to include deleted accounts.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// An <see cref="Account"/> with the specified username.
    /// </returns>
    Task<Account?> GetByUsernameAsync(
        string username,
        bool includeDeleted = false,
        bool includeInactive = false
    );

    /// <summary>
    /// Gets an account by its email.
    /// </summary>
    /// <param name="username">The email of the account.</param>
    /// <param name="includeDeleted">Whether to include deleted accounts.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// An <see cref="Account"/> with the specified email if found; otherwise, null.
    /// </returns>
    Task<Account?> GetByEmailAsync(
        string username,
        bool includeDeleted = false,
        bool includeInactive = false
    );

    /// <summary>
    /// Gets an account by its username or email.
    /// </summary>
    /// <param name="usernameOrEmail">The username or email of the account.</param>
    /// <param name="includeDeleted">Whether to include deleted accounts.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// An <see cref="Account"/> with the specified username or email if found; otherwise, null.
    /// </returns>
    Task<Account?> GetByUsernameOrEmailAsync(
        string usernameOrEmail,
        bool includeDeleted = false,
        bool includeInactive = false
    );

    /// <summary>
    /// Gets an account by its refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token of the account.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// An <see cref="Account"/> with the specified refresh token if found; otherwise, null.
    /// </returns>
    Task<Account?> GetByRefreshTokenAsync(string refreshToken, bool includeInactive = false);

    /// <summary>
    /// Gets accounts by their name.
    /// </summary>
    /// <param name="name">The name of the account.</param>
    /// <param name="includeDeleted">Whether to include deleted accounts.</param>
    /// <param name="includeInactive">Whether to include inactive accounts.</param>
    /// <returns>
    /// A collection of <see cref="Account"/> with the specified name.
    /// </returns>
    Task<ICollection<Account>> GetByNameAsync(
        string name,
        bool includeDeleted = false,
        bool includeInactive = false
    );

    /// <summary>
    /// Gets an account by its email verification code.
    /// </summary>
    /// <param name="confirmCode">The email verification code of the account.</param>
    /// <param name="verificationType">The verification type of the account.</param>
    /// <returns>
    /// An <see cref="Account"/> with the specified email verification code if found; otherwise, null.
    /// </returns>
    Task<Account?> GetByEmailVerifiedCode(string confirmCode, VerificationType verificationType);
}
